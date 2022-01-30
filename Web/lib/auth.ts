import {createContext, useCallback, useContext, useEffect, useState} from "react";
import jwt_decode, {JwtPayload} from "jwt-decode";
import Cookies from "js-cookie";
import {useRecoilState} from "recoil";
import {authState, UserToken} from "./appState";

interface AppJwt extends JwtPayload {
    unique_name: string;
}

function checkToken(token?: string): UserToken | null {
    if (!token) {
        return null;
    }
    try {
        let jwt = jwt_decode<AppJwt>(token!)
        if (new Date().getTime() / 1000 > jwt.exp!) {
            console.error("expired token");
            return null;
        } else {
            console.log(jwt);
            return {id: jwt.unique_name, jwt: token};
        }
    } catch (e) {
        // invalid token
        console.error("invalid stored token " + token + ": " + e);
        return null;
    }
}

export function useAuth(): [UserToken | null, (token: string) => void, () => void] {
    const [user, setUser] = useRecoilState<UserToken | null>(authState);

    useEffect(() => {
        if (user == null) setUser(checkToken(Cookies.get("token")));
    }, [user, setUser]);

    const setJwtToken = useCallback((token: string) => {
        Cookies.set("token", token, {
            expires: 7,
            sameSite: "strict"
        });
        setUser(checkToken(token));
    }, [setUser])

    useEffect(() => {
        if (window.location.hash.startsWith("#token=")) {
            let token = window.location.hash.substring("#token=".length);
            setJwtToken(token);
            window.location.hash = "";
        }
    }, [setJwtToken])

    function logout() {
        Cookies.remove("token");
        setUser(null);
    }

    return [user, setJwtToken, logout];
}

export const userHeaders = (user: UserToken | null) => {
    if (user === null) return {};
    return {
        headers: {
            Authorization: "Bearer " + user.jwt
        }
    }
};
