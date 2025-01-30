using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCodeWithPkceMauiApp.Security;

public record OAuthToken(string Value);

public record SmartMeOAuthTokens(
 OAuthToken AccessToken,
 OAuthToken RefreshToken);
