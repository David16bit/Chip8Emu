#pragma once
#include <winsock2.h>
#include <Windows.h>
#include "NetworkServices.h"
#include <ws2tcpip.h>
#include <map>
#include "NetworkData.h""
using namespace std;

#define DEFAULT_BUFLEN 512
#define DEFAULT_PORT "6881" 

class ServerNetwork
{

private:
	// Socket to listen for new connections
	SOCKET ListenSocket;

	// Socket to give to the clients
	SOCKET ClientSocket;

	// for error checking return values
	int iResult;



public:
	// table to keep track of each client's socket
	std::map<unsigned int, SOCKET> sessions;

	ServerNetwork(void);
	~ServerNetwork(void);
	// accept new connections
	bool acceptNewClient(unsigned int & id);
	// receive incoming data
	int receiveData(unsigned int client_id, char * recvbuf);
};