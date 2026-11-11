import type { IPublicClientApplication } from "@azure/msal-browser";
import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { apiScopes } from "./authConfig";

export type Me = {
  id: string;
  displayName: string;
  email: string | null;
  roles: string[];
  canSeeGiving: boolean;
};

/** Calls the API with the signed-in user's access token. */
export async function apiGet<T>(msal: IPublicClientApplication, path: string): Promise<T> {
  const account = msal.getActiveAccount() ?? msal.getAllAccounts()[0];
  if (!account) throw new Error("Not signed in");

  let token: string;
  try {
    token = (await msal.acquireTokenSilent({ scopes: apiScopes, account })).accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      await msal.acquireTokenRedirect({ scopes: apiScopes, account });
    }
    throw error;
  }

  const response = await fetch(path, { headers: { Authorization: `Bearer ${token}` } });
  if (!response.ok) throw new Error(`${path} returned ${response.status}`);
  return (await response.json()) as T;
}
