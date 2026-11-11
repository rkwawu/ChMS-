import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { EventType, PublicClientApplication, type AuthenticationResult } from "@azure/msal-browser";
import { MsalProvider } from "@azure/msal-react";
import App from "./App";
import { msalConfig } from "./authConfig";
import "./index.css";

const msal = new PublicClientApplication(msalConfig);

await msal.initialize();
await msal.handleRedirectPromise();

const [firstAccount] = msal.getAllAccounts();
if (!msal.getActiveAccount() && firstAccount) msal.setActiveAccount(firstAccount);

msal.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
    msal.setActiveAccount((event.payload as AuthenticationResult).account);
  }
});

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <MsalProvider instance={msal}>
      <App />
    </MsalProvider>
  </StrictMode>,
);
