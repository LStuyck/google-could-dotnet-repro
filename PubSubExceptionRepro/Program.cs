using Google.Cloud.PubSub.V1;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace PubSubExceptionRepro
{
    class Program
    {
        private const string ProjectId = "";
        private const string TopicId = "";
        private const string SubscriptionId = "";

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Main entry");

                await SubscribeToPubSub(ProjectId, SubscriptionId, TopicId);

                Console.WriteLine("Reached end");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        private static async Task SubscribeToPubSub(string projectId, string subscriptionId, string topicId)
        {
            SubscriberClient subscriber = null;

            try
            {
                Topic topic = await EnsureTopicExists(projectId, topicId);
                Subscription subscription = await EnsureSubscriptionExists(projectId, subscriptionId, topic.TopicName);

                subscriber = await SubscriberClient.CreateAsync(subscription.SubscriptionName);

                // Start the subscriber listening for messages.
                await subscriber.StartAsync((msg, cancellationToken) =>
                {
                    try
                    {
                        // Do stuff

                        // Return Reply.Ack to indicate this message has been handled.
                        return Task.FromResult(SubscriberClient.Reply.Ack);
                    }
                    catch (Exception ex)
                    {
                        return Task.FromResult(SubscriberClient.Reply.Nack);
                    }
                });
            }
            finally
            {
                if (subscriber != null)
                {
                    // Stop this subscriber after one message is received.
                    // This is non-blocking, and the returned Task may be awaited.
                    await subscriber.StopAsync(TimeSpan.FromSeconds(15));
                }
            }
        }

        private static async Task<Subscription> EnsureSubscriptionExists(string projectId, string subscriptionId, TopicName topicName)
        {
            // Subscribe to the topic.
            SubscriberServiceApiClient subscriberService = await SubscriberServiceApiClient.CreateAsync();
            SubscriptionName subscriptionName = new SubscriptionName(projectId, subscriptionId);
            Subscription subscription;

            try
            {
                subscription = await subscriberService.CreateSubscriptionAsync(subscriptionName, topicName, pushConfig: null, ackDeadlineSeconds: 60);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
                subscription = await subscriberService.GetSubscriptionAsync(subscriptionName);
            }

            return subscription;
        }

        private static async Task<Topic> EnsureTopicExists(string projectId, string topicId)
        {
            PublisherServiceApiClient publisher = await PublisherServiceApiClient.CreateAsync();
            var topicName = TopicName.FromProjectTopic(projectId, topicId);
            Topic topic;

            try
            {
                topic = await publisher.CreateTopicAsync(topicName);
            }
            catch (RpcException e) when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
                topic = await publisher.GetTopicAsync(topicName);
            }

            return topic;
        }
    }
}
