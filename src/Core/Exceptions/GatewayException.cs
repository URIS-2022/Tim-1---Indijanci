using System.Runtime.Serialization;

namespace Bit.Core.Exceptions;

[Serializable]
public class GatewayException : Exception
{
    protected GatewayException(SerializationInfo serializationInfo, StreamingContext streamingContext) 
        :base(serializationInfo, streamingContext)
    { }

    public GatewayException(string message, Exception innerException = null)
        : base(message, innerException)
    { }
}
