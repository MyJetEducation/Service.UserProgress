﻿using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.ServerKeyValue.Client;
using Service.ServiceBus.Models;
using Service.UserProgress.Jobs;
using Service.UserProgress.Services;

namespace Service.UserProgress.Modules
{
	public class ServiceModule : Module
	{
		private const string QueueName = "MyJetEducation-UserProgress";

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterServerKeyValueClient(Program.Settings.ServerKeyValueServiceUrl, Program.LogFactory.CreateLogger(typeof(ServerKeyValueClientFactory)));

			builder.RegisterType<UserProgressService>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<SetProgressInfoNotificator>().AutoActivate().SingleInstance();

			builder.RegisterType<KnowledgeProgressService>().AsSelf().SingleInstance();
			builder.RegisterType<HabitProgressService>().AsSelf().SingleInstance();
			builder.RegisterType<SkillProgressService>().AsImplementedInterfaces().SingleInstance();

			builder
				.Register(c => new List<IProgressDtoRepository>
				{
					c.Resolve<KnowledgeProgressService>(),
					c.Resolve<HabitProgressService>()
				})
				.As<IEnumerable<IProgressDtoRepository>>();

			MyServiceBusTcpClient serviceBusClient = builder.RegisterMyServiceBusTcpClient(Program.ReloadedSettings(e => e.ServiceBusReader), Program.LogFactory);
			builder.RegisterMyServiceBusSubscriberBatch<SetProgressInfoServiceBusModel>(serviceBusClient, SetProgressInfoServiceBusModel.TopicName, QueueName, TopicQueueType.Permanent);

			var tcpServiceBus = new MyServiceBusTcpClient(() => Program.Settings.ServiceBusWriter, "MyJetEducation Service.UserProgress");

			builder
				.Register(context => new MyServiceBusPublisher<UserProgressUpdatedServiceBusModel>(tcpServiceBus, UserProgressUpdatedServiceBusModel.TopicName, false))
				.As<IServiceBusPublisher<UserProgressUpdatedServiceBusModel>>()
				.SingleInstance();

			tcpServiceBus.Start();
		}
	}
}