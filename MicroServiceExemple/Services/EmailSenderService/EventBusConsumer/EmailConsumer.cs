using EventBus.Messages.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSenderService.EventBusConsumer
{
    public class EmailConsumer : IConsumer<EventBus.Messages.Events.SendEmailEvent>
    {
        private readonly Servises.IEmailService _emailService;
        public EmailConsumer(Servises.IEmailService emailService)
        {
            _emailService = emailService;
        }
        public async Task Consume(ConsumeContext<SendEmailEvent> context)
        {
            Console.WriteLine($"Sending Email\"{context.MessageId}\" ...");
            await _emailService.SendEmailAsync(context.Message.To, context.Message.Subject, context.Message.Body);
            Console.WriteLine($"Email\"{context.MessageId}\" sent successfully.");
        }
    }
}
