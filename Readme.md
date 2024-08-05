# Description

This is a utility allowing you to move from an Azure Blob Storage container to an AWS S3 bucket (optional folder).

Project can likely be modified to transfer the other way around (S3 to Azure), but our use-case was Azure to S3. 


## DISCLAIMER!!

This will incur transfer (egress and ingress) costs in Azure and AWS, as well as storage costs. Be sure to consult with pricing and documentation of those respective services. We are not responsible for any costs or side effects incurred. 

This has been tested on a container with over 55k blobs. It hasn't been stress tested beyond that. 

We make no guarantees as to outcome. Use at your own risk!

## Setup

Go to app settings and setup your azure and aws credentials.
Instructions are provided for the settings in this file. You should be able to figure it out from there.

After you set your appsettings, simply run the project and transfers will begin.


## Features

- Saves logs so that when you run it again, it will not try and transfer the same files twice if logging is enabled
- Even when a transfer is complete, you can run it again and it will transfer only newly added blobs

## Visit us at: www.iln.app

Get a FREE website to manage your business and build your brand!

Stay cool :)