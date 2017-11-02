#pragma once
#include <winsock2.h>
#include <Windows.h>
#include "ClientNetwork.h"
#include "NetworkData.h"
#include "NetworkServices.h"

class ClientApp
{

public:

	ClientApp();
	~ClientApp(void);

	void clientUpdate(bool flag);

	ClientNetwork* network;


};