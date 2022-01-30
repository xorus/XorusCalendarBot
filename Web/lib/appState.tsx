import {atom} from "recoil";
import {Guild} from "./apiObjects";

export interface UserToken {
    id: string;
    jwt: string;
}

export const authState = atom<UserToken | null>({
    key: 'auth',
    default: null
})

export const guildList = atom<Guild[]>({
    key: 'guildList',
    default: []
})
