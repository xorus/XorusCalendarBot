import {atom} from "recoil";

export interface UserToken {
    id: string;
    jwt: string;
}

export interface Guild {
    Id: string;
    IconUrl: string;
    Name: string;
}

export const authState = atom<UserToken | null>({
    key: 'auth',
    default: null
})

export const guildList = atom<Guild[]>({
    key: 'guildList',
    default: []
})
