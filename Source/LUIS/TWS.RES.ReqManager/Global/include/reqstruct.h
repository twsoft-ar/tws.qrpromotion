#ifndef __REQSTRUCT_H
#define __REQSTRUCT_H

#define REQ_HEADER_SIZE (sizeof(__int32) + sizeof(__int32) + sizeof(Byte))

#include "ByteStream.h"
#include "string.h" 

#define RES_OK				(1)
#define INVALID_MSG			(-1)

#define EXECUTION_ERROR		(-100)
#define CONNECTION_ERROR	(-101)
#define PARAM_ERROR			(-102)
#define TIMEOUT_ERROR		(-103)
#define CHECKSUM_ERROR		(-104)


#ifdef BYTE_ALIGN
	#undef BYTE_ALIGN
#endif

#if defined(UNDER_CE)
	#define BYTE_ALIGN
#else
	#define BYTE_ALIGN __declspec(align(1))
#endif

typedef struct BYTE_ALIGN tagReqMessage
{
	__int32 MessageType;
	__int32 BodySize;
	Byte	Checksum;
	Byte*	Body;

public:
	tagReqMessage()
	:	MessageType(INVALID_MSG),
		BodySize(0),
		Body(NULL),
		Checksum(0)
	{}
	
	tagReqMessage(long bodySize_)
	:	MessageType(INVALID_MSG),
		BodySize(bodySize_),
		Body(NULL),
		Checksum(0)
	{
		if(!(Body = new Byte[BodySize]))
			throw("ReqMessage::ReqMessage(long) -> buffer allocation error");
	}

	tagReqMessage(long bodySize_, Byte* body_)
	:	MessageType(INVALID_MSG),
		BodySize(bodySize_),
		Checksum(0)
	{
		if(!(Body = new Byte[BodySize]))
			throw("ReqMessage::ReqMessage(long) -> buffer allocation error");

		memcpy(Body, body_, bodySize_);
	}

	tagReqMessage(__int32 msgType_, long bodySize_, Byte* body_)
	:	MessageType(msgType_),
		BodySize(bodySize_)
	{
		if(!(Body = new Byte[BodySize]))
			throw("ReqMessage::ReqMessage(long) -> buffer allocation error");

		memcpy(Body, body_, bodySize_);
		GenerateChecksum();
	}

	tagReqMessage(const tagReqMessage& reqMsg_)
	{
		Body = new Byte[reqMsg_.BodySize];

		if(Body)
		{
			MessageType = reqMsg_.MessageType;
			BodySize	= reqMsg_.BodySize;
			Checksum	= reqMsg_.Checksum;

			memcpy(Body, reqMsg_.Body, BodySize);
		}
	}

	~tagReqMessage(void)
	{
		Clear();
	}

	tagReqMessage& operator= (const tagReqMessage& reqMsg_)
	{
		Clear();

		Body = new Byte[reqMsg_.BodySize];

		if(Body)
		{
			MessageType = reqMsg_.MessageType;
			BodySize	= reqMsg_.BodySize;
			Checksum	= reqMsg_.Checksum;
			memcpy(Body, reqMsg_.Body, BodySize);
		}
		else
			throw("ReqMessage::operator=() -> buffer allocation error");

		return (*this);
	}

	void Clear(void)
	{
		if(Body)
			delete[] Body;

		MessageType = INVALID_MSG;
		BodySize	= 0;
		Checksum	= 0;
		Body		= NULL;
	}

	bool IsValid(void) 
	{
		return (MessageType > INVALID_MSG);
	}

	void GenerateChecksum(void)
	{
		long i;
		Checksum = 0;

		//sum header
		for (i = 0; i < sizeof(MessageType); i++)
			Checksum += ((Byte*)&MessageType)[i];

		//sum header
		for(i = 0; i < sizeof(BodySize); i++)
			Checksum += ((Byte*)&BodySize)[i];

		for(i = 0; i < BodySize; i++)
			Checksum += Body[i];
	}

	ByteStream Bytes(void)
	{
		ByteStream res;

		//res = MessageType;
		res += ByteStream((Byte*)&MessageType, sizeof(MessageType));
		res += ByteStream((Byte*)&BodySize, sizeof(BodySize));
		res += ByteStream((Byte*)&Checksum, sizeof(Checksum));
		res += ByteStream((Byte*)Body, BodySize);
		//res += Checksum;

		return res;
	}

}ReqMessage;

#endif