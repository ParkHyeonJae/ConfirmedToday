#include <stdio.h>
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <locale.h>
#include <wchar.h>
#include <vector>
#include <iostream>

#pragma comment(lib, "ws2_32.lib")
#pragma warning(disable: 4996)

#define BUFFERSIZE 16384

char* ANSIToUTF8(const char* pszCode)
{
	int		nLength, nLength2;
	BSTR	bstrCode;
	char* pszUTFCode = NULL;

	nLength = MultiByteToWideChar(CP_ACP, 0, pszCode, strlen(pszCode), NULL, NULL);
	bstrCode = SysAllocStringLen(NULL, nLength);
	MultiByteToWideChar(CP_ACP, 0, pszCode, strlen(pszCode), bstrCode, nLength);

	nLength2 = WideCharToMultiByte(CP_UTF8, 0, bstrCode, -1, pszUTFCode, 0, NULL, NULL);
	pszUTFCode = (char*)malloc(nLength2 + 1);
	WideCharToMultiByte(CP_UTF8, 0, bstrCode, -1, pszUTFCode, nLength2, NULL, NULL);

	return pszUTFCode;
}

char* UTF8ToANSI(const char* pszCode)
{
	BSTR    bstrWide;
	char* pszAnsi;
	int     nLength;

	nLength = MultiByteToWideChar(CP_UTF8, 0, pszCode, strlen(pszCode) + 1, NULL, NULL);
	bstrWide = SysAllocStringLen(NULL, nLength);

	MultiByteToWideChar(CP_UTF8, 0, pszCode, strlen(pszCode) + 1, bstrWide, nLength);

	nLength = WideCharToMultiByte(CP_ACP, 0, bstrWide, -1, NULL, 0, NULL, NULL);
	pszAnsi = new char[nLength];

	WideCharToMultiByte(CP_ACP, 0, bstrWide, -1, pszAnsi, nLength, NULL, NULL);
	SysFreeString(bstrWide);

	return pszAnsi;
}

int main(void)
{
	WSADATA wsaData;
	SOCKET hSocket;
	SOCKADDR_IN servAddr;

	//setlocale(LC_ALL, "korean");
	if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) std::cout << "Failed WSAStartup() \n";

	hSocket = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (hSocket == INVALID_SOCKET) std::cout << "Failed Socket \n";

	memset(&servAddr, 0, sizeof(servAddr));

	servAddr.sin_family = AF_INET;

	inet_pton(AF_INET, "49.164.30.10", &servAddr.sin_addr);
	servAddr.sin_port = htons(39311);

	if (connect(hSocket, (SOCKADDR*)&servAddr, sizeof(servAddr)) == SOCKET_ERROR)
		std::cout << "Failed connect() \n";

	std::cout << "Connect Server\n";

	char sendBuff[256];
	std::string sendMessage;
	char recvBuff[BUFFERSIZE];
	char recvMessage[BUFFERSIZE];
	while (true)
	{
		memset(&sendBuff, 0, sizeof(sendBuff));
		memset(&recvMessage, 0, sizeof(recvMessage));
		memset(&recvBuff, 0, sizeof(recvBuff));

		std::cin >> sendBuff;
		sendMessage = std::move(ANSIToUTF8(std::move(sendBuff)));
		send(hSocket, sendMessage.c_str(), sendMessage.size(), 0);
		std::cout << "send bytes : " << sendMessage.size() << std::endl;
		
		int n = recv(hSocket, recvBuff, sizeof(recvBuff), 0);
		char* result = std::move(UTF8ToANSI(std::move(recvBuff)));
		std::cout << result << std::endl;
	}
	closesocket(hSocket);
	WSACleanup();

	return 0;
}