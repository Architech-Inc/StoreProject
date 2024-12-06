using System.Collections.ObjectModel;

namespace StoreProjectModels.Data
{
	public static class SiteSettings
	{
		public static string BaseUrl { get; set; } = "http://skillit-001-site1.btempurl.com";
		//public static string BaseUrl { get; set; } = "http://itskills.ovh";
		//public static string BaseUrl { get; set; } = "https://localhost:7078";
		public static string BaseUrl_Local_Secured { get; set; } = "https://localhost:7078";
		public static string BaseUrl_Local { get; set; } = "http://localhost:5165";
		public static string MailServer { get; } = "mail.itskills.ovh";
		public static string SecureMailServer { get; } = "mail5019.site4now.net";

		public static string AccountHelperEmailName { get; } = "Lofocam Account Helper";
		public static string InfoEmailName { get; } = "Lofocam Info";
		public static string SupportEmailName { get; } = "Lofocam Support";

		public static string AccountHelperEmail { get; } = "accounthelper@itskills.ovh";
		public static string InfoEmail { get; } = "info@itskills.ovh";
		public static string SupportEmail { get; } = "support@itskills.ovh";
		public static string AccountHelperEmailPassword { get; } = "@skillASP2023";
		public static string SendGrid_SMTP_API { get; } = "REDACTED_SENDGRID_KEY";
		public static int SSL_SMTP_Port { get; } = 465;
		public static int SMTP_Port_If_Google { get; } = 587;
		public static int SMTP_Port1 { get; } = 25;
		public static int SMTP_Port2 { get; } = 8889;

		/*public static string CreatePanelNotificationDetailMarkup1(Notification notification, Account account, ObservableCollection<NotificationAction> actions)
		{
			string acs = "";
			foreach (var item in actions)
			{
				string temp = $@"<a href=""{item.Command}"" title=""{item.Description}"" class=""relative text-white w-24 text-center bg-blue-600 hover:bg-blue-800 rounded px-2 sm:px-2"">
							{item.Name}
							</a>";
				acs += temp;
			}
			return $@"<div class=""notification-detail-card ndpn-card text-slate-800 rounded shadow-lg"">
				<div class=""ni-body"">
					<h4 class=""ni-caption"">{notification.Caption}</h4>
					<span class=""ni-detail"">{notification.Details}</span>
					<div class=""ni-body-actions"">
						{acs}
					</div>
				</div>
			</div>";
		}

		public static string CreatePageNotificationDetailMarkup1(Notification notification, Account account, ObservableCollection<NotificationAction> actions)
		{
			string acs = "";
			foreach (var item in actions)
			{
				string temp = $@"<a href=""{item.Command}"" title=""{item.Description}"" class=""relative text-white w-24 text-center bg-blue-600 hover:bg-blue-800 rounded px-2 sm:px-2"">
							{item.Name}
							</a>";
				acs += temp;
			}
			return $@"<div class=""notification-detail-card ndpg-card text-slate-800 rounded shadow-lg"">
				<div class=""ni-body"">
					<h4 class=""ni-caption"">{notification.Caption}</h4>
					<span class=""ni-detail"">{notification.Details}</span>
					<div class=""ni-body-actions"">
						{acs}
					</div>
				</div>
			</div>";
		}*/


		public static string GetWelcomeEmailTemplate(string name, string baseUrl)
		{
			return $@"<!doctype html>
			<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">

			<head>
				<title>

				</title>
				<!--[if !mso]><!-- -->
				<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
				<!--<![endif]-->
				<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
				<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
				<style type=""text/css"">
					#outlook a {{
						padding: 0;
					}}

					.ReadMsgBody {{
						width: 100%;
					}}

					.ExternalClass {{
						width: 100%;
					}}

					.ExternalClass * {{
						line-height: 100%;
					}}

					body {{
						margin: 0;
						padding: 0;
						-webkit-text-size-adjust: 100%;
						-ms-text-size-adjust: 100%;
					}}

					table,
					td {{
						border-collapse: collapse;
						mso-table-lspace: 0pt;
						mso-table-rspace: 0pt;
					}}

					img {{
						border: 0;
						height: auto;
						line-height: 100%;
						outline: none;
						text-decoration: none;
						-ms-interpolation-mode: bicubic;
					}}

					p {{
						display: block;
						margin: 13px 0;
					}}
				</style>
				<!--[if !mso]><!-->
				<style type=""text/css"">
					@media only screen and (max-width:480px) {{
						@-ms-viewport {{
							width: 320px;
						}}
						@viewport {{
							width: 320px;
						}}
					}}
				</style>
				<!--<![endif]-->
				<!--[if mso]>
					<xml>
					<o:OfficeDocumentSettings>
					  <o:AllowPNG/>
					  <o:PixelsPerInch>96</o:PixelsPerInch>
					</o:OfficeDocumentSettings>
					</xml>
					<![endif]-->
				<!--[if lte mso 11]>
					<style type=""text/css"">
					  .outlook-group-fix {{ width:100% !important; }}
					</style>
					<![endif]-->


				<style type=""text/css"">
					@media only screen and (min-width:480px) {{
						.mj-column-per-100 {{
							width: 100% !important;
						}}
					}}
				</style>


				<style type=""text/css"">
				</style>

			</head>

			<body style=""background-color:#f9f9f9;"">


				<div style=""background-color:#f9f9f9;"">


					<!--[if mso | IE]>
				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


					<div style=""background:#f9f9f9;background-color:#f9f9f9;Margin:0px auto;max-width:600px;"">

						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9f9f9;background-color:#f9f9f9;width:100%;"">
							<tbody>
								<tr>
									<td style=""border-bottom:#333957 solid 5px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
										<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
				
					<tr>
	  
					</tr>
	  
							  </table>
							<![endif]-->
									</td>
								</tr>
							</tbody>
						</table>

					</div>


					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>
	  
				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


					<div style=""background:#fff;background-color:#fff;Margin:0px auto;max-width:600px;"">

						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#fff;background-color:#fff;width:100%;"">
							<tbody>
								<tr>
									<td style=""border:#dddddd solid 1px;border-top:0px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
										<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
				
					<tr>
	  
						<td
						   style=""vertical-align:bottom;width:600px;""
						>
					  <![endif]-->

										<div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

											<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:bottom;"" width=""100%"">

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px;"">
															<tbody>
																<tr>
																	<td style=""width:200px;"">

																		<img height=""auto"" src=""{baseUrl}/assets/icons/logo.png"" style=""border:0;display:block;outline:none;text-decoration:none;width:100%;"" width=""64"" />

																	</td>
																</tr>
															</tbody>
														</table>

													</td>
												</tr>

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:40px;word-break:break-word;"">

														<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:28px;font-weight:bold;line-height:1;text-align:center;color:#555;"">
															Welcome to Lofocam
														</div>

													</td>
												</tr>

												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:16px;line-height:22px;text-align:left;color:#555;"">
															Hello {name}!<br></br>
															Thank you for signing up on the Lofocam website. We're really happy to have you! Click the link below to login to your account:
														</div>

													</td>
												</tr>

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-top:30px;padding-bottom:50px;word-break:break-word;"">

														<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:separate;line-height:100%;"">
															<tr>
																<td align=""center"" bgcolor=""#2F67F6"" role=""presentation"" style=""border:none;border-radius:3px;color:#ffffff;cursor:auto;padding:15px 25px;"" valign=""middle"">
																	<a href=""{baseUrl}/?signin=email"" target=""_blank"" style=""padding-block:10px;display:flex;background:#2F67F6;color:#ffffff;font-family:'Helvetica Neue',Arial,sans-serif;font-size:15px;font-weight:normal;Margin:0;text-decoration:none;text-transform:none;"">
																		Login to your account
																	</a>
																</td>
															</tr>
														</table>

													</td>
												</tr>

												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;color:#525252;"">
															Best regards,<br><br>Lofocam Educate, Account Helper<br>
															<a href=""{baseUrl}"" style=""color:#2F67F6"">{baseUrl}</a>
														</div>

													</td>
												</tr>

											</table>

										</div>

										<!--[if mso | IE]>
						</td>

					</tr>

							  </table>
							<![endif]-->
									</td>
								</tr>
							</tbody>
						</table>

					</div>


					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>

				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


					<div style=""Margin:0px auto;max-width:600px;"">

						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""width:100%;"">
							<tbody>
								<tr>
									<td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
										<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

					<tr>

						<td
						   style=""vertical-align:bottom;width:600px;""
						>
					  <![endif]-->

										<div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

											<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" width=""100%"">
												<tbody>
													<tr>
														<td style=""vertical-align:bottom;padding:0;"">

															<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" width=""100%"">

																<tr>
																	<td align=""center"" style=""font-size:0px;padding:0;word-break:break-word;"">

																		<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:12px;font-weight:300;line-height:1;text-align:center;color:#575757;"">
																			Cameroon (CM), Africa
																		</div>

																	</td>
																</tr>

															</table>

														</td>
													</tr>
												</tbody>
											</table>

										</div>

										<!--[if mso | IE]>
						</td>

					</tr>

							  </table>
							<![endif]-->
									</td>
								</tr>
							</tbody>
						</table>

					</div>


					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>
				  <![endif]-->


				</div>

			</body>

			</html>";
		}

		public static string GetConfirmEmailTemplate(string link, string ac_email, string supportEmail, string baseUrl)
		{
			return $@"<!doctype html>
		<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">

		<head>
			<title>

			</title>
			<!--[if !mso]><!-- -->
			<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
			<!--<![endif]-->
			<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
			<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
			<style type=""text/css"">
				#outlook a {{
					padding: 0;
				}}

				.ReadMsgBody {{
					width: 100%;
				}}

				.ExternalClass {{
					width: 100%;
				}}

				.ExternalClass * {{
					line-height: 100%;
				}}

				body {{
					margin: 0;
					padding: 0;
					-webkit-text-size-adjust: 100%;
					-ms-text-size-adjust: 100%;
				}}

				table,
				td {{
					border-collapse: collapse;
					mso-table-lspace: 0pt;
					mso-table-rspace: 0pt;
				}}

				img {{
					border: 0;
					height: auto;
					line-height: 100%;
					outline: none;
					text-decoration: none;
					-ms-interpolation-mode: bicubic;
				}}

				p {{
					display: block;
					margin: 13px 0;
				}}
			</style>
			<!--[if !mso]><!-->
			<style type=""text/css"">
				@media only screen and (max-width:480px) {{
					@-ms-viewport {{
						width: 320px;
					}}
					@viewport {{
						width: 320px;
					}}
				}}
			</style>
			<!--<![endif]-->
			<!--[if mso]>
				<xml>
				<o:OfficeDocumentSettings>
					<o:AllowPNG/>
					<o:PixelsPerInch>96</o:PixelsPerInch>
				</o:OfficeDocumentSettings>
				</xml>
				<![endif]-->
			<!--[if lte mso 11]>
				<style type=""text/css"">
					.outlook-group-fix {{ width:100% !important; }}
				</style>
				<![endif]-->


			<style type=""text/css"">
				@media only screen and (min-width:480px) {{
					.mj-column-per-100 {{
						width: 100% !important;
					}}
				}}
			</style>


			<style type=""text/css"">
			</style>

		</head>

		<body style=""background-color:#f9f9f9;"">


			<div style=""background-color:#f9f9f9;"">


				<!--[if mso | IE]>
				<table
					align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				>
				<tr>
					<td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				<![endif]-->


				<div style=""background:#f9f9f9;background-color:#f9f9f9;Margin:0px auto;max-width:600px;"">

					<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9f9f9;background-color:#f9f9f9;width:100%;"">
						<tbody>
							<tr>
								<td style=""border-bottom:#333957 solid 5px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
									<!--[if mso | IE]>
							<table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

				<tr>

				</tr>

							</table>
						<![endif]-->
								</td>
							</tr>
						</tbody>
					</table>

				</div>


				<!--[if mso | IE]>
					</td>
				</tr>
				</table>

				<table
					align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				>
				<tr>
					<td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				<![endif]-->


				<div style=""background:#fff;background-color:#fff;Margin:0px auto;max-width:600px;"">

					<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#fff;background-color:#fff;width:100%;"">
						<tbody>
							<tr>
								<td style=""border:#dddddd solid 1px;border-top:0px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
									<!--[if mso | IE]>
							<table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

				<tr>

					<td
						style=""vertical-align:bottom;width:600px;""
					>
					<![endif]-->

									<div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

										<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:bottom;"" width=""100%"">

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

													<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px;"">
														<tbody>
															<tr>
																<td style=""width:200px;"">

																	<img height=""auto"" src=""{baseUrl}/assets/icons/logo.png""
																		style=""border:0;display:block;outline:none;text-decoration:none;width:100%;"" width=""64"" />

																</td>
															</tr>
														</tbody>
													</table>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:40px;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:32px;font-weight:bold;line-height:1;text-align:center;color:#555;"">
														Please confirm your email
													</div>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:0;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:16px;line-height:22px;text-align:center;color:#555;"">
														Yes, we know.
													</div>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:16px;line-height:22px;text-align:center;color:#555;"">
														An email to confirm an email. 🤪
													</div>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:20px;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:16px;line-height:22px;text-align:center;color:#555;"">
														Please validate your email address in order to get started with enrolling subjects.
													</div>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-top:30px;padding-bottom:40px;word-break:break-word;"">

													<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:separate;line-height:100%;"">
														<tr>
															<td align=""center"" bgcolor=""#2F67F6"" role=""presentation"" style=""border:none;border-radius:3px;color:#ffffff;cursor:auto;padding:15px 25px;"" valign=""middle"">
																<a href=""{link}"" target=""_blank"" style=""background:#2F67F6;color:#ffffff;font-family:'Helvetica Neue',Arial,sans-serif;font-size:15px;font-weight:normal;line-height:120%;Margin:0;text-decoration:none;text-transform:none;"">
																	Confirm Your Email
																</a>
															</td>
														</tr>
													</table>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:0;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:16px;line-height:22px;text-align:center;color:#555;"">
														Or verify using this link:
													</div>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:40px;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:16px;line-height:22px;text-align:center;color:#555;"">
														<a href=""{link}"" style=""color:#2F67F6"">{link}</a>
													</div>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:26px;font-weight:bold;line-height:1;text-align:center;color:#555;"">
														Need Help?
													</div>

												</td>
											</tr>

											<tr>
												<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

													<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:22px;text-align:center;color:#555;"">
														Please send a feedback or report a bug info<br> to <a href=""mailto:{supportEmail}"" style=""color:#2F67F6"">{supportEmail}</a>
													</div>

												</td>
											</tr>

										</table>

									</div>

									<!--[if mso | IE]>
					</td>

				</tr>

							</table>
						<![endif]-->
								</td>
							</tr>
						</tbody>
					</table>

				</div>


				<!--[if mso | IE]>
					</td>
				</tr>
				</table>

				<table
					align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				>
				<tr>
					<td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				<![endif]-->

		<div style=""Margin:0px auto;max-width:600px;"">

			<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""width:100%;"">
				<tbody>
					<tr>
						<td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
							<!--[if mso | IE]>
							<table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

				<tr>

					<td
						style=""vertical-align:bottom;width:600px;""
					>
					<![endif]-->

							<div class=""mj-column-per-100 outlook-group-fix""
								style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

								<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" width=""100%"">
									<tbody>
										<tr>
											<td style=""vertical-align:bottom;padding:0;"">

												<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation""
													width=""100%"">

													<tr>
														<td align=""center""
															style=""font-size:0px;padding:0;word-break:break-word;"">

															<div
																style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:12px;font-weight:300;line-height:1;text-align:center;color:#575757;"">
																Cameroon (CM), Africa
															</div>

														</td>
													</tr>

												</table>

											</td>
										</tr>
									</tbody>
								</table>

							</div>

							<!--[if mso | IE]>
					</td>

				</tr>

							</table>
						<![endif]-->
						</td>
					</tr>
				</tbody>
			</table>

		</div>

				<!--[if mso | IE]>
					</td>
				</tr>
				</table>
				<![endif]-->


			</div>

		</body>

		</html>";
		}

		public static string GetResetEmailTemplate(string link, string baseUrl)
		{
			return $@"<!doctype html>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">

<head>
	<title>

	</title>
	<!--[if !mso]><!-- -->
	<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
	<!--<![endif]-->
	<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
	<style type=""text/css"">
		#outlook a {{
			padding: 0;
		}}

		.ReadMsgBody {{
			width: 100%;
		}}

		.ExternalClass {{
			width: 100%;
		}}

		.ExternalClass * {{
			line-height: 100%;
		}}

		body {{
			margin: 0;
			padding: 0;
			-webkit-text-size-adjust: 100%;
			-ms-text-size-adjust: 100%;
		}}

		table,
		td {{
			border-collapse: collapse;
			mso-table-lspace: 0pt;
			mso-table-rspace: 0pt;
		}}

		img {{
			border: 0;
			height: auto;
			line-height: 100%;
			outline: none;
			text-decoration: none;
			-ms-interpolation-mode: bicubic;
		}}

		p {{
			display: block;
			margin: 13px 0;
		}}
	</style>
	<!--[if !mso]><!-->
	<style type=""text/css"">
		@media only screen and (max-width:480px) {{
			@-ms-viewport {{
				width: 320px;
			}}
			@viewport {{
				width: 320px;
			}}
		}}
	</style>
	<!--<![endif]-->
	<!--[if mso]>
		<xml>
		<o:OfficeDocumentSettings>
		  <o:AllowPNG/>
		  <o:PixelsPerInch>96</o:PixelsPerInch>
		</o:OfficeDocumentSettings>
		</xml>
		<![endif]-->
	<!--[if lte mso 11]>
		<style type=""text/css"">
		  .outlook-group-fix {{ width:100% !important; }}
		</style>
		<![endif]-->


	<style type=""text/css"">
		@media only screen and (min-width:480px) {{
			.mj-column-per-100 {{
				width: 100% !important;
			}}
		}}
	</style>


	<style type=""text/css"">
	</style>

</head>

<body style=""background-color:#f9f9f9;"">


	<div style=""background-color:#f9f9f9;"">


		<!--[if mso | IE]>
	  <table
		 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
	  >
		<tr>
		  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
	  <![endif]-->


		<div style=""background:#f9f9f9;background-color:#f9f9f9;Margin:0px auto;max-width:600px;"">

			<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9f9f9;background-color:#f9f9f9;width:100%;"">
				<tbody>
					<tr>
						<td style=""border-bottom:#333957 solid 5px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
							<!--[if mso | IE]>
				  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

		<tr>

		</tr>

				  </table>
				<![endif]-->
						</td>
					</tr>
				</tbody>
			</table>

		</div>


		<!--[if mso | IE]>
		  </td>
		</tr>
	  </table>

	  <table
		 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
	  >
		<tr>
		  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
	  <![endif]-->


		<div style=""background:#fff;background-color:#fff;Margin:0px auto;max-width:600px;"">

			<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#fff;background-color:#fff;width:100%;"">
				<tbody>
					<tr>
						<td style=""border:#dddddd solid 1px;border-top:0px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
							<!--[if mso | IE]>
				  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

		<tr>

			<td
			   style=""vertical-align:bottom;width:600px;""
			>
		  <![endif]-->

							<div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

								<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:bottom;"" width=""100%"">

									<tr>
										<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

											<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px;"">
												<tbody>
													<tr>
														<td style=""width:200px;"">

															<img height=""auto"" src=""{baseUrl}/assets/icons/logo.png""
																style=""border:0;display:block;outline:none;text-decoration:none;width:100%;"" width=""64"" />

														</td>
													</tr>
												</tbody>
											</table>

										</td>
									</tr>

									<tr>
										<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:40px;word-break:break-word;"">

											<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:38px;font-weight:bold;line-height:1;text-align:center;color:#555;"">
												Oops!
											</div>

										</td>
									</tr>

									<tr>
										<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:40px;word-break:break-word;"">

											<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:18px;line-height:1;text-align:center;color:#555;"">
												It seems that you’ve forgotten your password.
											</div>

										</td>
									</tr>

									<tr>
										<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-top:30px;padding-bottom:50px;word-break:break-word;"">

											<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:separate;line-height:100%;"">
												<tr>
													<td align=""center"" bgcolor=""#2F67F6"" role=""presentation"" style=""border:none;border-radius:3px;color:#ffffff;cursor:auto;padding:15px 25px;"" valign=""middle"">
														<a href=""{link}"" style=""background:#2F67F6;color:#ffffff;font-family:'Helvetica Neue',Arial,sans-serif;font-size:15px;font-weight:normal;line-height:120%;Margin:0;text-decoration:none;text-transform:none;"">
															Reset Password
														</a>
													</td>
												</tr>
											</table>

										</td>
									</tr>

									<tr>
										<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-bottom:40px;word-break:break-word;"">

											<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:16px;line-height:20px;text-align:center;color:#7F8FA4;"">
												If you did not make this request, just ignore this email. Otherwise please click the button above to reset your password.
											</div>

										</td>
									</tr>

								</table>

							</div>

							<!--[if mso | IE]>
			</td>

		</tr>

				  </table>
				<![endif]-->
						</td>
					</tr>
				</tbody>
			</table>

		</div>


		<!--[if mso | IE]>
		  </td>
		</tr>
	  </table>

	  <table
		 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
	  >
		<tr>
		  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
	  <![endif]-->

<div style=""Margin:0px auto;max-width:600px;"">

	<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""width:100%;"">
		<tbody>
			<tr>
				<td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
					<!--[if mso | IE]>
				  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

		<tr>

			<td
			   style=""vertical-align:bottom;width:600px;""
			>
		  <![endif]-->

					<div class=""mj-column-per-100 outlook-group-fix""
						style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

						<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" width=""100%"">
							<tbody>
								<tr>
									<td style=""vertical-align:bottom;padding:0;"">

										<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation""
											width=""100%"">

											<tr>
												<td align=""center""
													style=""font-size:0px;padding:0;word-break:break-word;"">

													<div
														style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:12px;font-weight:300;line-height:1;text-align:center;color:#575757;"">
														Cameroon (CM), Africa
													</div>

												</td>
											</tr>

										</table>

									</td>
								</tr>
							</tbody>
						</table>

					</div>

					<!--[if mso | IE]>
			</td>

		</tr>

				  </table>
				<![endif]-->
				</td>
			</tr>
		</tbody>
	</table>

</div>


		<!--[if mso | IE]>
		  </td>
		</tr>
	  </table>
	  <![endif]-->


	</div>

</body>

</html>
			";
		}

		public static string GetSubjectRegisteredEmail(string subjectName, string startDate, string link, string ux_email, string feedbackLink, string baseUrl)
		{
			return $@"<!doctype html>
			<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">

			<head>
				<title>

				</title>
				<!--[if !mso]><!-- -->
				<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
				<!--<![endif]-->
				<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
				<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
				<style type=""text/css"">
					#outlook a {{
						padding: 0;
					}}

					.ReadMsgBody {{
						width: 100%;
					}}

					.ExternalClass {{
						width: 100%;
					}}

					.ExternalClass * {{
						line-height: 100%;
					}}

					body {{
						margin: 0;
						padding: 0;
						-webkit-text-size-adjust: 100%;
						-ms-text-size-adjust: 100%;
					}}

					table,
					td {{
						border-collapse: collapse;
						mso-table-lspace: 0pt;
						mso-table-rspace: 0pt;
					}}

					img {{
						border: 0;
						height: auto;
						line-height: 100%;
						outline: none;
						text-decoration: none;
						-ms-interpolation-mode: bicubic;
					}}

					p {{
						display: block;
						margin: 13px 0;
					}}
				</style>
				<!--[if !mso]><!-->
				<style type=""text/css"">
					@media only screen and (max-width:480px) {{
						@-ms-viewport {{
							width: 320px;
						}}
						@viewport {{
							width: 320px;
						}}
					}}
				</style>
				<!--<![endif]-->
				<!--[if mso]>
					<xml>
					<o:OfficeDocumentSettings>
					  <o:AllowPNG/>
					  <o:PixelsPerInch>96</o:PixelsPerInch>
					</o:OfficeDocumentSettings>
					</xml>
					<![endif]-->
				<!--[if lte mso 11]>
					<style type=""text/css"">
					  .outlook-group-fix {{ width:100% !important; }}
					</style>
					<![endif]-->


				<style type=""text/css"">
					@media only screen and (min-width:480px) {{
						.mj-column-per-100 {{
							width: 100% !important;
						}}
					}}
				</style>


				<style type=""text/css"">
				</style>

			</head>

			<body style=""background-color:#f9f9f9;"">


				<div style=""background-color:#f9f9f9;"">


					<!--[if mso | IE]>
				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


					<div style=""background:#f9f9f9;background-color:#f9f9f9;Margin:0px auto;max-width:600px;"">

						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9f9f9;background-color:#f9f9f9;width:100%;"">
							<tbody>
								<tr>
									<td style=""border-bottom:#333957 solid 5px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
										<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

					<tr>

					</tr>

							  </table>
							<![endif]-->
									</td>
								</tr>
							</tbody>
						</table>

					</div>


					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>

				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


					<div style=""background:#fff;background-color:#fff;Margin:0px auto;max-width:600px;"">

						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#fff;background-color:#fff;width:100%;"">
							<tbody>
								<tr>
									<td style=""border:#dddddd solid 1px;border-top:0px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
										<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

					<tr>

						<td
						   style=""vertical-align:bottom;width:600px;""
						>
					  <![endif]-->

										<div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

											<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:bottom;"" width=""100%"">

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px;"">
															<tbody>
																<tr>
																	<td style=""width:200px;"">

																		<img height=""auto"" src=""{baseUrl}/assets/icons/logo.png""
																			style=""border:0;display:block;outline:none;text-decoration:none;width:100%;"" width=""64"" />

																	</td>
																</tr>
															</tbody>
														</table>

													</td>
												</tr>


												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:24px;font-weight:bold;line-height:22px;text-align:center;color:#525252;"">
															Thank you for enrolling
														</div>

													</td>
												</tr>

												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:22px;text-align:left;color:#525252;"">
															<p>You successfully enrolled to our <b>{subjectName}</b> subject this day <b>{startDate}</b>. 
																Hoping you will learn a lot from this subject, for we are so glad you are taking the subject with us. </p>
														</div>

													</td>
												</tr>

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-top:30px;padding-bottom:50px;word-break:break-word;"">

														<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:separate;line-height:100%;"">
															<tr>
																<td align=""center"" bgcolor=""#2F67F6"" role=""presentation"" style=""border:none;border-radius:3px;color:#ffffff;cursor:auto;padding:15px 25px;"" valign=""middle"">
																	<p style=""background:#2F67F6;color:#ffffff;font-family:'Helvetica Neue',Arial,sans-serif;font-size:15px;font-weight:normal;line-height:120%;Margin:0;text-decoration:none;text-transform:none;"">
																		<a href=""{link}"" style=""color:#fff; text-decoration:none"">
					  Continue learning</a>
																	</p>
																</td>
															</tr>
														</table>

													</td>
												</tr>

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
									
														<div
															style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:24px;font-weight:bold;line-height:22px;text-align:center;color:#525252;"">
															Let us know your experience
														</div>
									
													</td>
												</tr>
									
												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
									
														<div
															style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:22px;text-align:left;color:#525252;"">
															<p>
																Please leave us a feedback to tell us more about your experiences because we would love to here from our users.
																<br/>
																<center>You can leave us an email <br/> <a href=""mailto:{ux_email}"" style=""color:#2F67F6"">{ux_email}</a>
																<br/> or via our website
																<br> <a href=""{feedbackLink}"" style=""color:#2F67F6"">{feedbackLink}</a></center>
															</p>
														</div>

													</td>
												</tr>

												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div
															style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;color:#525252;"">
															Happy learning,<br><br>Lofocam Educate, Educate Team<br>
															<a href=""{baseUrl}"" style=""color:#2F67F6"">{baseUrl}</a>
														</div>

													</td>
												</tr>

											</table>

										</div>

										<!--[if mso | IE]>
						</td>

					</tr>

							  </table>
							<![endif]-->
									</td>
								</tr>
							</tbody>
						</table>

					</div>


					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>

				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


			<div style=""Margin:0px auto;max-width:600px;"">

				<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""width:100%;"">
					<tbody>
						<tr>
							<td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
								<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

					<tr>

						<td
						   style=""vertical-align:bottom;width:600px;""
						>
					  <![endif]-->

								<div class=""mj-column-per-100 outlook-group-fix""
									style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

									<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" width=""100%"">
										<tbody>
											<tr>
												<td style=""vertical-align:bottom;padding:0;"">

													<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation""
														width=""100%"">

														<tr>
															<td align=""center""
																style=""font-size:0px;padding:0;word-break:break-word;"">

																<div
																	style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:12px;font-weight:300;line-height:1;text-align:center;color:#575757;"">
																	Cameroon (CM), Africa
																</div>

															</td>
														</tr>

													</table>

												</td>
											</tr>
										</tbody>
									</table>

								</div>

								<!--[if mso | IE]>
						</td>

					</tr>

							  </table>
							<![endif]-->
							</td>
						</tr>
					</tbody>
				</table>

			</div>

					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>
				  <![endif]-->


				</div>

			</body>

			</html>";
		}

		public static string GetSubjectCompletedEmail(string subjectName, string completionDate, string link, string ux_email, string feedbackLink, string baseUrl)
		{
			return $@"<!doctype html>
			<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"">

			<head>
				<title>

				</title>
				<!--[if !mso]><!-- -->
				<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
				<!--<![endif]-->
				<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
				<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
				<style type=""text/css"">
					#outlook a {{
						padding: 0;
					}}

					.ReadMsgBody {{
						width: 100%;
					}}

					.ExternalClass {{
						width: 100%;
					}}

					.ExternalClass * {{
						line-height: 100%;
					}}

					body {{
						margin: 0;
						padding: 0;
						-webkit-text-size-adjust: 100%;
						-ms-text-size-adjust: 100%;
					}}

					table,
					td {{
						border-collapse: collapse;
						mso-table-lspace: 0pt;
						mso-table-rspace: 0pt;
					}}

					img {{
						border: 0;
						height: auto;
						line-height: 100%;
						outline: none;
						text-decoration: none;
						-ms-interpolation-mode: bicubic;
					}}

					p {{
						display: block;
						margin: 13px 0;
					}}
				</style>
				<!--[if !mso]><!-->
				<style type=""text/css"">
					@media only screen and (max-width:480px) {{
						@-ms-viewport {{
							width: 320px;
						}}
						@viewport {{
							width: 320px;
						}}
					}}
				</style>
				<!--<![endif]-->
				<!--[if mso]>
					<xml>
					<o:OfficeDocumentSettings>
					  <o:AllowPNG/>
					  <o:PixelsPerInch>96</o:PixelsPerInch>
					</o:OfficeDocumentSettings>
					</xml>
					<![endif]-->
				<!--[if lte mso 11]>
					<style type=""text/css"">
					  .outlook-group-fix {{ width:100% !important; }}
					</style>
					<![endif]-->


				<style type=""text/css"">
					@media only screen and (min-width:480px) {{
						.mj-column-per-100 {{
							width: 100% !important;
						}}
					}}
				</style>


				<style type=""text/css"">
				</style>

			</head>

			<body style=""background-color:#f9f9f9;"">


				<div style=""background-color:#f9f9f9;"">


					<!--[if mso | IE]>
				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


					<div style=""background:#f9f9f9;background-color:#f9f9f9;Margin:0px auto;max-width:600px;"">

						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#f9f9f9;background-color:#f9f9f9;width:100%;"">
							<tbody>
								<tr>
									<td style=""border-bottom:#333957 solid 5px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
										<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

					<tr>

					</tr>

							  </table>
							<![endif]-->
									</td>
								</tr>
							</tbody>
						</table>

					</div>


					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>

				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


					<div style=""background:#fff;background-color:#fff;Margin:0px auto;max-width:600px;"">

						<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#fff;background-color:#fff;width:100%;"">
							<tbody>
								<tr>
									<td style=""border:#dddddd solid 1px;border-top:0px;direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
										<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

					<tr>

						<td
						   style=""vertical-align:bottom;width:600px;""
						>
					  <![endif]-->

										<div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

											<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:bottom;"" width=""100%"">

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px;"">
															<tbody>
																<tr>
																	<td style=""width:200px;"">

																		<img height=""auto"" src=""{baseUrl}/assets/icons/logo.png""
																			style=""border:0;display:block;outline:none;text-decoration:none;width:100%;"" width=""64"" />

																	</td>
																</tr>
															</tbody>
														</table>

													</td>
												</tr>


												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:24px;font-weight:bold;line-height:22px;text-align:center;color:#525252;"">
															Congratulations
														</div>

													</td>
												</tr>

												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:22px;text-align:left;color:#525252;"">
															<p>You successfully completed <b>{subjectName}</b> this day <b>{completionDate}</b>. 
																Hoping you have learnt a lot from this subject, for we are so glad you took the subject with us. </p>
														</div>

													</td>
												</tr>

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;padding-top:30px;padding-bottom:50px;word-break:break-word;"">

														<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:separate;line-height:100%;"">
															<tr>
																<td align=""center"" bgcolor=""#2F67F6"" role=""presentation"" style=""border:none;border-radius:3px;color:#ffffff;cursor:auto;padding:15px 25px;"" valign=""middle"">
																	<p style=""background:#2F67F6;color:#ffffff;font-family:'Helvetica Neue',Arial,sans-serif;font-size:15px;font-weight:normal;line-height:120%;Margin:0;text-decoration:none;text-transform:none;"">
																		<a href=""{link}"" style=""color:#fff; text-decoration:none"">
					  Continue learning</a>
																	</p>
																</td>
															</tr>
														</table>

													</td>
												</tr>

												<tr>
													<td align=""center"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
									
														<div
															style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:24px;font-weight:bold;line-height:22px;text-align:center;color:#525252;"">
															Let us know your experience
														</div>
									
													</td>
												</tr>
									
												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">
									
														<div
															style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:22px;text-align:left;color:#525252;"">
															<p>
																Please leave us a feedback to tell us more about your experiences because we would love to here from our users.
																<br/>
																<center>You can leave us an email <br/> <a href=""mailto:{ux_email}"" style=""color:#2F67F6"">{ux_email}</a>
																<br/> or via our website
																<br> <a href=""{feedbackLink}"" style=""color:#2F67F6"">{feedbackLink}</a></center>
															</p>
														</div>

													</td>
												</tr>

												<tr>
													<td align=""left"" style=""font-size:0px;padding:10px 25px;word-break:break-word;"">

														<div
															style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:14px;line-height:20px;text-align:left;color:#525252;"">
															Happy learning,<br><br>Lofocam Educate, Educate Team<br>
															<a href=""{baseUrl}"" style=""color:#2F67F6"">{baseUrl}</a>
														</div>

													</td>
												</tr>

											</table>

										</div>

										<!--[if mso | IE]>
						</td>

					</tr>

							  </table>
							<![endif]-->
									</td>
								</tr>
							</tbody>
						</table>

					</div>


					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>

				  <table
					 align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:600px;"" width=""600""
				  >
					<tr>
					  <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
				  <![endif]-->


			<div style=""Margin:0px auto;max-width:600px;"">

				<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""width:100%;"">
					<tbody>
						<tr>
							<td style=""direction:ltr;font-size:0px;padding:20px 0;text-align:center;vertical-align:top;"">
								<!--[if mso | IE]>
							  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">

					<tr>

						<td
						   style=""vertical-align:bottom;width:600px;""
						>
					  <![endif]-->

								<div class=""mj-column-per-100 outlook-group-fix""
									style=""font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;"">

									<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" width=""100%"">
										<tbody>
											<tr>
												<td style=""vertical-align:bottom;padding:0;"">

													<table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation""
														width=""100%"">

														<tr>
															<td align=""center""
																style=""font-size:0px;padding:0;word-break:break-word;"">

																<div
																	style=""font-family:'Helvetica Neue',Arial,sans-serif;font-size:12px;font-weight:300;line-height:1;text-align:center;color:#575757;"">
																	Cameroon (CM), Africa
																</div>

															</td>
														</tr>

													</table>

												</td>
											</tr>
										</tbody>
									</table>

								</div>

								<!--[if mso | IE]>
						</td>

					</tr>

							  </table>
							<![endif]-->
							</td>
						</tr>
					</tbody>
				</table>

			</div>

					<!--[if mso | IE]>
					  </td>
					</tr>
				  </table>
				  <![endif]-->


				</div>

			</body>

			</html>";
		}
	}
}
