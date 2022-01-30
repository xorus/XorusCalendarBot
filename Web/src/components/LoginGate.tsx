import {useRecoilState} from "recoil";
import {authState, UserToken} from "../../lib/appState";
import React, {ReactElement} from "react";
import {Heading} from "theme-ui";

export const LoginGate = ({children}: { children: ReactElement }) => {
    const [user] = useRecoilState<UserToken | null>(authState);

    if (user) {
        return children;
    }
    return <Heading>
        Login is required to access this content.
    </Heading>;
}