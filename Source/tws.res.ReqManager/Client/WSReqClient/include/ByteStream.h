#ifndef __BYTESTREAM_H
#define __BYTESTREAM_H

typedef unsigned char Byte;

//LAST MODIFICATION: 01-03-07

class ByteStream
	{
	private:
		Byte* mBytes;
		size_t mLength;

	public:
		ByteStream();
		ByteStream(Byte byte_);
		ByteStream(char* bytes_);
		ByteStream(const char* bytes_);
		ByteStream(Byte* bytes_, size_t length_);
		ByteStream(const ByteStream& byteStream_);
		~ByteStream();

		void Assign(Byte* bytes_, size_t length_);
		Byte* Bytes(void);
		size_t Length(void) const;

		void Clear(void);

		bool operator ==(const ByteStream& byteStream_);
		ByteStream& operator =(const ByteStream& byteStream_);
		ByteStream& operator =(const Byte byte_);

		ByteStream operator +(const ByteStream& byteStream_);
		ByteStream operator +(const Byte byte_);

		friend ByteStream operator +(const Byte byte_, const ByteStream& byteStream_);
		friend ByteStream operator +(const ByteStream& byteStream_, const Byte byte_);
		friend ByteStream operator +(const ByteStream& byteStream_, const ByteStream& byteStream2_);

		ByteStream& operator +=(const ByteStream byteStream_);
		ByteStream& operator +=(const Byte byte_);

		Byte& operator[] (const size_t index_);
		Byte  operator[] (const size_t index_) const;

		operator char*() const;
		operator Byte*() const;
	};

#endif