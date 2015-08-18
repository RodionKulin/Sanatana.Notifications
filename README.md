SignaloBot
=========

SignaloBot is a c# notification framework for your application subscribers. It uses MsSql as storage but easily modified to use your prefered storage.

Use SignaloBot.Sender to launch a separate process like Console App or Windows Service for sending messages from queue. Like Email, Sms, Push or whatever.

Also use SignaloBot.Client for managing your subscribers notification settings and enqueueing new messages for them to database queue.

And SignaloBot.NDR to hahdle NDR reports of messages sent. Some sending providers like Amazon SES provide support to handling NDR.

Amazon SES implementation for SignaloBot.Sender and SignaloBot.NDR is also included as example of integration of verious sending methods.


Installation
=========
Use installer project to create database tables if you want to use MsSql database structure provided.
