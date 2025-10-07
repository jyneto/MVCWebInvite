namespace MVCWebInvite.Utils
{
    public interface IAuthorizedClientProvider
    {
        HttpClient GetClient();
        HttpClient GetAnonClient();
       
    }
    
}
