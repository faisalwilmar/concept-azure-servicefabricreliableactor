# Concept.Azure.ServiceFabricReliableActor

## About

This project is a proof-of-concept Virtual Actors implementation prepared for the Service Fabric cluster running on Azure.

------

## Acceptance Criteria

1. Service Fabric application containing following services:

   a. *UserProfileService* – Reliable Actor Service with storage backed by CosmosDB Collection 

   b. *UserInterestsAPIService* – Stateless Web API Service with Kestrel HTTP endpoint

2. Service Fabric application can be deployed to local 5 nodes cluster, exposing HTTP endpoint to port 8000

3. UserProfileService implementation is the following: 

   a. Instances are identified by GUIDs 

   b. During activation, profile is loaded from CosmosDB Collection (see Implementation Notes) 

   c. Active instance exposes following async methods: 

   ​	i. string[] GetInterests() 

   ​	ii. void AddInterests(string[]) 

   d. During deactivation, interests are written to CosmosDB Table

4. UserInterestsAPIService implementation is the following: 

   a. Service is stateless

   b. Service exposes following sync methods: 

   ​	i. GET /users/{GUID}/interests/ < [‘gaming’, ‘shopping’] 

   ​	ii. PATCH /users/{GUID}/interests/ > [‘realty’] < [‘gaming’, ‘shopping’, ‘realty’] 

   c. Service communicates with UserProfileService using fastest endpoint available

------

## How to Run

[Service Fabric SDK](https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-get-started) is need to be installed and running for the **Local Cluster**.

Azure Cosmos DB Account is required with following condition:

1. Have database with Database Id *Advertisement*
2. Have container in *Advertisement* with Container Id *UserInterest* and Partition Key being set to `/partitionKey`

Modify the `ServiceManifest.xml` in both project. Set the Cosmos DB Environment Variable. You can search for `<EnvironmentVariable Name="CosmosDB" Value=""/>`. Replace the **value** with your Cosmos DB Account's *Connection String*.

------

## References

https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-7.0

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-connect-and-communicate-with-services#connecting-to-other-services

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-manifest-example-reliable-services-app

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-overview

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-actors-get-started

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-actors-introduction

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-actors-lifecycle

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-actors-notes-on-actor-type-serialization

https://learn.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-introduction

https://stackoverflow.com/questions/39647952/service-fabric-actor-service-dependency-injection-and-actor-events