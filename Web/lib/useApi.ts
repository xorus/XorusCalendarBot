import {useRecoilState} from "recoil";
import {useAuth, userHeaders} from "./auth";
import {useCallback, useEffect, useState} from "react";
import {apiUrl} from "./apiUrl";
import {guildList, UserToken} from "./appState";
import {Guild} from "./apiObjects";

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
    const [user, _, logout] = useAuth();

    const refreshGuilds = useCallback(async () => {
        if (!user) return;
        let guilds = await jsonReq({
            url: apiUrl('/api/user/guilds'),
            user
        });
        if (guilds === null) {
            logout();
            return;
        }
        setGuilds(guilds);
    }, [user, setGuilds, logout]);

    useEffect(() => {
        if (user == null || guilds.length > 0) return;
        refreshGuilds();
    }, [user, guilds, refreshGuilds]);

    return [guilds, refreshGuilds];
}