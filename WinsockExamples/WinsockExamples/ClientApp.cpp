#include "stdafx.h"
#include "ClientApp.h" 
#include <stdio.h>
#include <string.h>

ClientApp::ClientApp(void)
{
	network = new ClientNetwork();
}

void ClientApp::clientUpdate(bool flag)
{
	// get new clients
	// send init packet
	if (flag) {
		const unsigned int packet_size = sizeof(Packet);
		char packet_data[packet_size];

		Packet packet;
		packet.packet_type = ACTION_EVENT;
		strcpy_s(packet.dataBuffer, "copy successful");

		packet.serialize(packet_data);

		NetworkServices::sendMessage(network->ConnectSocket, packet_data, packet_size);
	}
}

