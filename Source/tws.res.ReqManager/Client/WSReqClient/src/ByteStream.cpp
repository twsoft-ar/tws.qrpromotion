#include "ByteStream.h"
#include "string.h"

ByteStream::ByteStream()
{
	mBytes = NULL;
	mLength = 0;
}

ByteStream::ByteStream(Byte* bytes_, size_t length_)
{
	mBytes = NULL;
	Assign(bytes_, length_);
}

ByteStream::ByteStream(Byte byte_)
{
	mBytes = NULL;
	Byte byte = byte_;
	Assign(&byte, 1);
}

ByteStream::ByteStream(char* bytes_)
{
	mBytes = NULL;
	Assign((Byte*)bytes_, strlen(bytes_));
}

ByteStream::ByteStream(const char* bytes_)
{
	mBytes = NULL;
	Assign((Byte*)bytes_, strlen(bytes_));
}

ByteStream::ByteStream(const ByteStream& byteStream_)
{
	mBytes = NULL;
	Assign(byteStream_.mBytes, byteStream_.mLength);
}

ByteStream::~ByteStream()
{
	try
	{
		if(mBytes)
			delete mBytes;
	
		mBytes = NULL;
		mLength = 0;
	}
	catch(...)
	{
		throw("ByteStream::~ByteStream()-> delete mBytes");
	}

}

void ByteStream::Assign(Byte* bytes_, size_t length_)
{
	try
	{
		if(mBytes)
			delete mBytes;

		mBytes = NULL;
		mLength = 0;

		if(length_ > 0)
		{
			if(mBytes = new Byte[length_])
			{
				mLength = length_;
				for(size_t i=0; i<length_; i++)
					*(mBytes+i) = *(bytes_+i);
			}
		}
	}
	catch(...)
	{
		throw("ByteStream::Assign-> delete mBytes");
	}
}

Byte* ByteStream::Bytes(void)
{
	return mBytes;
}

size_t ByteStream::Length(void) const
{
	return mLength;
}

void ByteStream::Clear(void)
{
	try
	{
		if(mBytes)
			delete mBytes;

		mBytes = NULL;
		mLength = 0;
	}
	catch(...)
	{
		throw("ByteStream::Borrar-> delete mBytes");
	}
}

bool ByteStream::operator ==(const ByteStream& byteStream_)
{
	bool res = false;

	if(res = (mLength == byteStream_.mLength))
	{
		for(size_t i=0; res && i<mLength; i++)
			res = ((*(mBytes + i) == *(byteStream_.mBytes+i)));
	}

	return res;
}

ByteStream& ByteStream::operator =(const ByteStream& byteStream_)
{
	Assign(byteStream_.mBytes, byteStream_.mLength);

	return *this;
}

ByteStream& ByteStream::operator =(const Byte byte_)
{
	Byte byte = byte_;
	Assign(&byte, 1);

	return *this;
}

ByteStream ByteStream::operator +(const ByteStream& byteStream_)
{
	ByteStream res;
	size_t i;

	if(res.mBytes = new Byte[mLength + byteStream_.mLength])
	{
		res.mLength = mLength + byteStream_.mLength;
	
		for(i=0; i<mLength; i++)
		{
			res.mBytes[i] = mBytes[i];
		}
	
		for(i=0; i< byteStream_.mLength; i++)
		{
			res.mBytes[i+mLength] = byteStream_.mBytes[i];
		}
	}

	return res;
}

ByteStream ByteStream::operator +(const Byte byte_)
{
	ByteStream res;
	res = operator +(ByteStream((Byte*)&byte_,1));
	return res;
}

ByteStream operator +(const Byte byte_, const ByteStream& byteStream_)
{
	ByteStream res(byte_);
	return res + byteStream_;
}

ByteStream operator +(const ByteStream& byteStream_, const Byte byte_)
{
	ByteStream res(byte_);
	return byteStream_ + res;
}

ByteStream operator +(const ByteStream& byteStream_, const ByteStream& byteStream2_)
{
	ByteStream res = byteStream_;
	
	res = res + byteStream2_;
	return res;
}

Byte& ByteStream::operator[] (const size_t index_)
{
	return mBytes[index_];
}

Byte ByteStream::operator[] (const size_t index_) const
{
	return mBytes[index_];
}

ByteStream& ByteStream::operator +=(const ByteStream byteStream_)
{
	*this = *this + byteStream_;
	return *this;
}

ByteStream& ByteStream::operator +=(const Byte byte_)
{
	*this = *this + ByteStream((Byte*)&byte_,1);
	return *this;
}

ByteStream::operator char*() const
{
	return (char*)mBytes;
}

ByteStream::operator Byte*() const
{
	return mBytes;
}
