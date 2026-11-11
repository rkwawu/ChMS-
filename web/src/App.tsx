import { useEffect, useState } from "react";
import { AuthenticatedTemplate, UnauthenticatedTemplate, useMsal } from "@azure/msal-react";
import { apiGet, type Me } from "./api";
import { apiScopes } from "./authConfig";

function SignedIn() {
  const { instance } = useMsal();
  const [me, setMe] = useState<Me | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    apiGet<Me>(instance, "/api/me").then(setMe, (e: unknown) => setError(String(e)));
  }, [instance]);

  if (error) return <p role="alert">Couldn't load your profile: {error}</p>;
  if (!me) return <p>Loading…</p>;

  return (
    <section>
      <h2>Welcome, {me.displayName}</h2>
      <dl>
        <dt>Email</dt>
        <dd>{me.email ?? "—"}</dd>
        <dt>Roles</dt>
        <dd>{me.roles.length ? me.roles.join(", ") : "No roles assigned yet. Ask an administrator."}</dd>
        <dt>Can see giving amounts</dt>
        <dd>{me.canSeeGiving ? "Yes" : "No"}</dd>
      </dl>
      <button onClick={() => instance.logoutRedirect()}>Sign out</button>
    </section>
  );
}

export default function App() {
  const { instance } = useMsal();

  return (
    <main>
      <h1>ChMS</h1>
      <AuthenticatedTemplate>
        <SignedIn />
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <p>Sign in with your church Microsoft 365 account.</p>
        <button onClick={() => instance.loginRedirect({ scopes: apiScopes })}>Sign in</button>
      </UnauthenticatedTemplate>
    </main>
  );
}
