using System.Runtime.InteropServices;
using Nncase.Runtime.Interop;

namespace Nncase.Compiler;

public static class PythonHelper
{
    public static IValue TensorValueFromBytes(DataType type, byte[] span, int[] dimensions)
    {
        return Value.FromTensor(Tensor.FromBytes(type, span, dimensions));
    }
    
    public static Tensor TensorFromBytes(DataType type, byte[] span, int[] dimensions)
    {
        return Tensor.FromBytes(type, span, dimensions);
    }


    public static byte[] BytesBufferFromTensor(Tensor value)
    {
        return value.BytesBuffer.ToArray();
    }

    public static Memory<byte> ToMemory(byte[] bytes) => new(bytes);

    public static byte[] GetRTTensorBytes(RTTensor tensor)
    {
        var buffer = tensor.Buffer.Buffer.AsHost()!;
        using (var mmOwner = buffer.Map(RTMapAccess.Read))
        {
            return mmOwner.Memory.Span.ToArray();
        }
    }
    
    public static uint[] GetRTTensorDims(RTTensor tensor)
    {
        return tensor.Dimensions.ToArray();
    }
    
    public static RTTensor[] RunSimulator(RTInterpreter interp, RTValue[] input)
    {
        var entry = interp.Entry;
        var result = (RTTensor)entry.Invoke(input);
        if (result is RTTensor tensor)
        {
            return new[] {tensor};
        }

        throw new NotImplementedException();
    }
}