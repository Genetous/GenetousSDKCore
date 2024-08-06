## Genetous .Net Framework SDK ##

### Kurulum: ###

Paketi https://www.nuget.org/packages/GenetousSDK adresinden size uygun bir seçenek ile kurun.

### Değişkenleri Belirleme: ###

```csharp
applicationVariables.applicationId = "your appId";
applicationVariables.organizationId = "your orgId";
applicationVariables.clientSecret = "your client secret";
applicationVariables.host = "http://localhost/";
```

### Örnek Login işlemi: ###

```csharp
 Dictionary<string, object> data = new Dictionary<string, object>();
 data.Add("userpass", "your pass");
 data.Add("usermail", "your mail");
 object res = await new PostGetBuilder()
     .setData(data)
     .build()
     .login();
    string retData = "";
    if (res is string)
    {
    retData = res.ToString();
    }
    else if (res is Dictionary<string, object>)
    {
        Dictionary<string, object> resDic = (Dictionary<string, object>)res;
        if (resDic.ContainsKey("success") && ((bool)resDic["success"]) == true)
        {
            retData = resDic["token"].ToString();
        }
        else
        {
            retData = "Something went wrong!";
        }
    }
```