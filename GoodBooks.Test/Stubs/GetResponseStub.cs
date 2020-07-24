using Nest;

namespace GoodBooks.Test.Stubs
{
    public class GetResponseStub<T> : GetResponse<T> where T : class
    {
        public GetResponseStub(bool found, T source = default)
        {
            GetType().GetProperty(nameof(Found)).SetMethod.Invoke(this, new object[] {found});
            GetType().GetProperty(nameof(Source)).SetMethod.Invoke(this, new object[] {source});
        }
    }
}