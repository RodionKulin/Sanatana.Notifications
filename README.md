SignaloBot
=========

SignaloBot is a c# notification service for your application subscribers. It uses MongoDb as data storage but can be modified in SignaloBot.DAL to use your prefered storage.

Use SignaloBot.Sender to compose messages from custom created templetes and deliver them to your subscribers. It's possible to send Email, Sms, Mobile Push or whatever kind of messages you need. Email Sender is provided out of the box, but a small class with your sending method is required to implement for other sending types.

SignlaoBot.Sender launches a WCF server that receives notification events. To enqueue new notification events into SignlaoBot.Sender pipeline create a WCF proxy classes from SignlaoBot.Sender WCF metadata and use it's endpoint in your application code.

To launch SignaloBot.Sender use a separate process like Console App or Windows Service that will be a background worker.

Use SignaloBot.DAL for managing your subscribers notification settings from your application code.

Use SignaloBot.NDR to handle NDR reports of messages sent. Some sending providers like Amazon SES provide support to handling NDR.

To launch SignaloBot.NDR use a separate process like Console App or Windows Service.

Amazon SES implementation for SignaloBot.Sender and SignaloBot.NDR is also included as example of integration of various sending methods.


Installation
=========
Use SignaloBot.Initializer project to create database tables if you want to use MongoDb database structure provided.

