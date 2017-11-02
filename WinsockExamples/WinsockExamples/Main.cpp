// WinsockExamples.cpp : Defines the entry point for the console application.
//

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <iphlpapi.h>
#include <stdio.h>
#include "stdafx.h"
#include "ClientApp.h"
#include "ServerApp.h"
#include "NetworkData.h"
#include "NetworkServices.h"
// used for multi-threading
#include <process.h>


#pragma comment(lib, "Ws2_32.lib")

ServerApp* server;
ClientApp* client;
ClientApp* client2;
static unsigned int client_id = 0;

void serverLoop(void *);
void clientLoop(void);

int main()
{
	server = new ServerApp();
	// create thread with arbitrary argument for the run function
	_beginthread(serverLoop, 0, (void*)12);

	client = new ClientApp();
	client2 = new ClientApp();
	clientLoop();

    return 0;
}



void serverLoop(void * arg)
{
	while (true)
	{
		server->update();
	}

}

void clientLoop()
{
	bool val = true;
	while (true)
	{
		//do game stuff
		//will later run client->update();
		client->clientUpdate(val);
		val = false;
	}
}

