# smart-me OAuth Samples

## Supported Grants (Flows)

| Name  | Use case |
| ------------- | ------------- |
| Authorization Code  | Cloud Application with a Backend |
| Authorization Code with PKCE  | Public Application like a web app or an App without a Backend  |
| Device Code  | IoT Device with a Display  |
| Client Credentials  | IoT Device without a Display  |

## URLs
| Name  | Url |
| ------------- | ------------- |
| Base Url  | https://api.smart-me.com |
| Authorization  | /oauth/authorize |
| Token  | /oauth/token |
| Device Code | /oauth/device |


# Samples

## Authorization Code with PKCE (.net Maui Example)

## Flow
1. App makes an auth request and shows the login page
   
   ![image](https://github.com/user-attachments/assets/9a723952-6aa5-4e74-9de8-7c3b62f99941)

3. User accept that this app can access to the smart-me data
   
![image](https://github.com/user-attachments/assets/2dfdee8f-4da7-4751-95f4-9aff34bc18e9)

5. The register deeplink (mysampleapp://callback/ in the sample) is called and the access and refresh token is returned.
   
### Setup

#### Create oAuth Application
1. Go to https://www.smart-me.com and login with your smart-me Account
2. Go to the API Settings and create an "Public" OAuth Application. As redirect URL choose the deep link defined in the Maui App

![image](https://github.com/user-attachments/assets/9baf54ad-ebdf-4ca0-929c-b060a56e8966)



## Device Code
The device code can be used to give a IoT Device access to the smart-me Api. This without the need to enter and store the username and password on the device.

## Flow
1. Device makes an authorize call to /oauth/device
2. The device shows the device code on the screen (e.g. 0648-9465-6349)
3. The user goes to https://api.smart-me.com/oauth/verify
4. The user logs in with his smart-me account:
   
   ![image](https://github.com/user-attachments/assets/3e47a5b5-201b-4bfc-8a4f-f91272af36be)
6. The user enters the code from the device
   
   ![image](https://github.com/user-attachments/assets/0751d9ef-0a1c-4e67-9d5a-f0e1d0679b75)
8. The user accecpt that the device will has access to his data:
   
   ![image](https://github.com/user-attachments/assets/4764e721-ea67-401a-a3ab-e6132bac63eb)
10. The device gets the Access Token and can use that for the API


### Setup 
#### Create oAuth Application
1. Go to https://www.smart-me.com and login with your smart-me Account
2. Go to the API Settings and create an "Public" OAuth Application
![image](https://github.com/user-attachments/assets/4ba19d44-c975-4acb-9cbf-c0f9edacb8fc)
3. Copy the ClientId into your device firmware



