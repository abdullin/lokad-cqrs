using System;
using System.Linq;
using System.Web.Mvc;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using Lokad;
using Lokad.Quality;
using Lokad.Rules;
using Sample_05.Contracts;

namespace Sample_05.Web.Controllers
{
	/// <summary>Controls user login/logout, plus registration.</summary>
	[HandleError]
	public class UserController : Controller
	{
		const string OpenIdFormKey = "openId";
		
		static readonly OpenIdRelyingParty OpenId = new OpenIdRelyingParty();

		[UsedImplicitly]
		public ActionResult Index()
		{
			Response.AppendHeader(
				"X-XRDS-Location",
				new Uri(Request.Url, Response.ApplyAppPathModifier("~/user/xrds")).AbsoluteUri);

			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("index", "home");
			}

			return RedirectToAction("login");
		}

		[UsedImplicitly]
		public ActionResult Xrds()
		{
			return View("xrds");
		}

		public ActionResult Logout()
		{
			GlobalAuth.LogoutAndRedirectToLogin();
			// we would not get here
			return RedirectToAction("login");
		}

		//[RequireSsl(Redirect = true)]
		public ActionResult Login()
		{
			// Stage 1: display login form to user
			return View("login");
		}


		[ValidateInput(false), UsedImplicitly]
		public ActionResult Authenticate(string returnUrl)
		{
			var response = OpenId.GetResponse();
			if (response == null)
			{
				// Stage 2: user submitting Identifier
				return UserSubmitsIdentifier();
			}
			// Stage 3: OpenID Provider sending assertion response
			return OpenIdReturnsResponse(response, returnUrl);
		}

		ActionResult OpenIdReturnsResponse(IAuthenticationResponse response, string returnUrl)
		{
			switch (response.Status)
			{
				case AuthenticationStatus.Authenticated:
					var claimedIdentifier = response.ClaimedIdentifier.ToString();

					var id = GlobalAuth.AuthenticateOpenId(claimedIdentifier);
					if (id.HasValue)
					{
						return LoginAndRedirect(id.Value, returnUrl);
					}

					var form = new RegistrationForm
						{
							Identity = claimedIdentifier,
							Name = response.FriendlyIdentifierForDisplay,
						};
					return View("Register", form);


				case AuthenticationStatus.Canceled:
					ViewData["Message"] = "Canceled at provider";
					return View("Login");
				case AuthenticationStatus.Failed:
					ViewData["Message"] = response.Exception.Message;
					return View("Login");
			}
			return new EmptyResult();
		}

		ActionResult LoginAndRedirect(SessionIdentity sessionIdentity, string returnUrl)
		{
			GlobalAuth.HandleLogin(sessionIdentity, false);
			if (string.IsNullOrEmpty(returnUrl))
			{
				return RedirectToAction("index", "home");
			}
			return Redirect(returnUrl);
		}

		ActionResult UserSubmitsIdentifier()
		{
			Identifier id;
			if (Identifier.TryParse(Request.Form[OpenIdFormKey], out id))
			{
				try
				{
					string location = OpenId.CreateRequest(Request.Form[OpenIdFormKey]).RedirectingResponse.Headers["Location"];
					return Redirect(location);
				}
				catch (ProtocolException ex)
				{
					ViewData["Message"] = ex.Message;
					return View("login");
				}
			}

			ViewData["Message"] = "Invalid identifier";
			return View("login");
		}

		static void ValidForm(RegistrationForm model, IScope scope)
		{
			scope.Validate(() => model.Name, StringIs.Limited(2, 256));
			scope.Validate(() => model.Email, StringIs.ValidEmail);
		}


		//[RequireSsl(Redirect = true)]
		public ActionResult Register(RegistrationForm form)
		{
			ModelState.ValidateErrors(form, ValidForm);

			if (ModelState.IsValid)
			{
				var userId = GuidUtil.NewComb();

				AzureEsb.SendCommand(new RegisterUserCommand(
					userId,
					form.Identity,
					form.Name,
					Request.UserHostAddress,
					form.Email));

				return View("wait", userId);
			}
			return View("register", form);
		}

		public ActionResult CheckJson(Guid userId)
		{
			var result = AzureViews.Get<UserView>(userId);
			if (!result.HasValue)
			{
				return Json(new
					{
						Status = "Waiting for the registration..."
					});
			}
			var view = result.Value;
			var identity = new SessionIdentity(new AuthInfo(view.UserId), view.Username);
			GlobalAuth.HandleLogin(identity, false);

			return Json(new
				{
					Status = "Registered. Redirecting...",
					Redirect = Url.Action("index", "wizard")
				});
		}
	}

	public sealed class RegistrationForm
	{
		public string Identity { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
	}

	public static class ExtendModelState
	{
		public static void ValidateErrors<T>(this ModelStateDictionary state, T item, params Rule<T>[] rules)
		{
			SetNullStringsAsEmpty(item);

			using (var scope = new ModelStateAdapterScope(state, ""))
			{
				scope.ValidateInScope(item, rules);
			}
		}

		static void SetNullStringsAsEmpty<T>(T item)
		{
			foreach (var info in typeof(T).GetProperties().Where(p => p.PropertyType == typeof(string)))
			{
				if (info.GetValue(item, null) == null)
				{
					info.SetValue(item, "", null);
				}
			}
		}
	}

	sealed class ModelStateAdapterScope : IScope
	{
		readonly ModelStateDictionary _dictionary;
		readonly string _path;

		public ModelStateAdapterScope(ModelStateDictionary dictionary, string path)
		{
			_dictionary = dictionary;
			_path = path;
		}

		public ModelStateAdapterScope(ModelStateDictionary dictionary)
		{
			_dictionary = dictionary;
			_path = "";
		}

		public void Dispose()
		{
		}

		IScope IScope.Create(string name)
		{
			if (String.IsNullOrEmpty(_path))
			{
				return new ModelStateAdapterScope(_dictionary, name);
			}
			else
			{
				return new ModelStateAdapterScope(_dictionary, Scope.ComposePath(_path, name));
			}
		}

		public void Write(RuleLevel level, string message)
		{
			if (level == RuleLevel.Error)
			{
				_dictionary.AddModelError(_path, message);
			}
		}

		public RuleLevel Level
		{
			get { return _dictionary.IsValid ? RuleLevel.None : RuleLevel.Error; }
		}
	}

	[Authorize]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return new EmptyResult();
		}
	}


}