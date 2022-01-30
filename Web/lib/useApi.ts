import {useRecoilState} from "recoil";
import {useAuth, userHeaders} from "./auth";
import {useCallback, useEffect} from "react";
import {apiUrl} from "./apiUrl";
import {Guild, guildList, UserToken} from "./appState";

export const jsonReq = async ({url, user, method = "GET", body}: {
    url: string,
    user: UserToken,
    method?: string,
    body?: string,
}) => {
    const r = await fetch(url, {...userHeaders(user), method: method, body: body});
    if (r.ok) return r.json();
    console.error("server error occured: " + r.status + " " + r.statusText + " ");
    return null;
}

export function useGuilds(): [Guild[], (() => void)] {
    const [guilds, setGuilds] = useRecoilState<Guild[]>(guildList);
    const [user] = useAuth();

    const refreshGuilds = useCallback(async () => {
        user && setGuilds(await jsonReq({
            url: apiUrl('/api/user/guilds'),
            user
        }) ?? []);
    }, [user, setGuilds]);

    useEffect(() => {
        if (user == null || guilds.length > 0) return;
        refreshGuilds();
    }, [user, guilds, refreshGuilds]);

    return [guilds, refreshGuilds];
}