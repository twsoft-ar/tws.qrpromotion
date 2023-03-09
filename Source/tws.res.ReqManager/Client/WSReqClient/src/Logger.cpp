// Utils.cpp: implementation of the Utils class.
//
//////////////////////////////////////////////////////////////////////

#ifdef _MFC_
//#include "stdafx.h"
#include <afxwin.h>
#else 
#include "Windows.h"
#endif

#include "Logger.h"

unsigned long  Logger::mLocked = UTILSSTATE::LOCKED;
unsigned long  Logger::mUnlocked = UTILSSTATE::UNLOCKED;
unsigned long* Logger::mLockingState = &mUnlocked;


lev::string Logger::GetTimestamp(bool getMs_)
{
	//lev::string retVal;
	SYSTEMTIME	dateTime;
	char		timeStr[256];

	GetLocalTime(&dateTime);

	if (getMs_)
		sprintf(timeStr, "%04d/%02d/%02d - %02d:%02d:%02d.%03d->", dateTime.wYear, dateTime.wMonth, dateTime.wDay, dateTime.wHour, dateTime.wMinute, dateTime.wSecond, dateTime.wMilliseconds);
	else
		sprintf(timeStr, "%04d/%02d/%02d - %02d:%02d:%02d->", dateTime.wYear, dateTime.wMonth, dateTime.wDay, dateTime.wHour, dateTime.wMinute, dateTime.wSecond);

	return lev::string(timeStr);
}

void Logger::LogEvent(lev::string fileName_, lev::string logMessage_, bool append_)
{
	std::ofstream logFile;

	try
	{
		Lock();
	
		if(append_)
			logFile.open((char*)fileName_, std::ios_base::binary | std::ios_base::app);
		else
			logFile.open((char*)fileName_, std::ios_base::binary);
		
		if(logFile.is_open())
			logFile	<< (char*)GetTimestamp() << (char*)logMessage_ << std::endl ;
	}
	catch(...)
	{
	}

	Unlock();

	if(logFile.is_open())
	{		
		logFile.close();
		logFile.flush();
	}
}
