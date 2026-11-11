import { type Configuration, LogLevel } from "@azure/msal-browser";

// Values come from web/.env.local (see .env.example). They are public identifiers, not secrets.
const tenantId = import.meta.env.VITE_ENTRA_TENANT_ID;
const clientId = import.meta.env.VITE_ENTRA_SPA_CLIENT_ID;

export const msalConfig: Configuration = {
  auth: {
    clientId,
    authority: `https://login.microsoftonline.com/${tenantId}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: "sessionStorage",
  },
  system: {
    loggerOptions: {
      logLevel: LogLevel.Warning,
      loggerCallback: (_level, message, containsPii) => {
        if (!containsPii) console.warn(message);
      },
    },
  },
};

/** The API scope the SPA asks for, e.g. api://<api-client-id>/access_as_user */
export const apiScopes = [import.meta.env.VITE_API_SCOPE];
