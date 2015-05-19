# boids-demo
A boids demo for stormancer where each Boid is simulated by a different connected player.

#Projects
The repository includes the following projects:

##Server 
Contains the Stormancer server implementation. Deploy it on a Stormancer app with the following commands

    git remote add stormancer http://api.stormancer.com/_r/<accountId>/<appName>.git
    git subtree push --prefix server stormancer master


##BoidsClient
This library contains the Boids implementation

##BoidsClient.Cmd
An executable that can start local boid clients.

##BoidsUnity
Unity3D vizualization application

#Running the demo
##On my test application
1. Compile and start BoidsClient.Cmd to connect boid clients to the server (Beware that as all boids are simulated on different client instances, Wifi (or high number of clients) is not recommanded to save your network bandwidth.)
2. Compile and run the Unity app to vizualize the simulation.
##On your own server

1. Install the server application on a Stormancer cluster (create a free account & app on the public server is a good way to get started.)
2. Change the app informations in BoidsClient.Cmd/peer.cs, compile and start BoidsClient.Cmd to connect boid clients to the server (Beware that as all boids are simulated on different client instances, Wifi is not recommanded.)
3. Change the app informations in BoidsUnity/GameEngine.cs, compile and run the Unity app to vizualize the simulation.
