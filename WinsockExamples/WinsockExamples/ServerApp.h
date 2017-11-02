#pragma once
#include "ServerNetwork.h"
#include "NetworkData.h"
#include "NetworkServices.h"

class ServerApp
{

public:
	// The ServerNetwork object 
	ServerNetwork* network;

	ServerApp(void);
	~ServerApp(void);

	void update();
	void receiveFromClients();

private:

	// IDs for the clients connecting for table in ServerNetwork 
	static unsigned int client_id;

	// data buffer
	char network_data[MAX_PACKET_SIZE];
};