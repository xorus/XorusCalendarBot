import React, {useCallback, useEffect, useState} from "react";
import {Avatar, Box, Button, Divider, Flex, Heading, Link, Spinner} from "theme-ui";
import {apiUrl} from "../../lib/apiUrl";
import {useAuth} from "../../lib/auth";
import {useTheme} from "@theme-ui/style-guide";
import {jsonReq, useGuilds} from "lib/useApi";
import {Calendar, CalendarEntity} from "../components/Calendar";
import {GuildBanner} from "../components/GuildBanner";

const CalendarPage = () => {
    const [user] = useAuth();
    const [calendars, setCalendars] = useState<CalendarEntity[] | null>();
    const [guilds] = useGuilds();
    const theme = useTheme();

    const refresh = useCallback(async () => {
        console.log("req calendars " + JSON.stringify(user))
        const json = await jsonReq({url: apiUrl("/api/calendar"), user: user!});
        json && setCalendars(json);
    }, [user]);

    useEffect(() => {
        user && refresh()
    }, [user, refresh]);

    if (user == null) return null;

    let calendarsPerGuild: Map<string, CalendarEntity[]> = new Map<string, CalendarEntity[]>();
    let g: React.ReactNode[] = [];
    calendars?.forEach(c => {
        if (g.length > 0) g.push(<Divider/>);
        let col = calendarsPerGuild.get(c.GuildId) ?? [];
        col.push(c);
        calendarsPerGuild.set(c.GuildId, col);
    });

    calendarsPerGuild.forEach((collection, guildId) => {
        g.push(<>
            <GuildBanner guild={guilds.find((g) => g.Id === guildId)}/>
            <Flex variant={"layout.guildCalendars"}>
                {collection ?
                    collection.map(c => <Calendar key={c.Id} calendar={c} user={user} refresh={() => refresh()}
                                                  defaultOpen={false && collection.length === 1}
                    />) : <Box><Spinner/></Box>}
                <Button type={"button"} onClick={async () => {
                    await jsonReq({
                        url: apiUrl("/api/calendar", {guild: guildId}, {}),
                        method: "POST",
                        body: "",
                        user: user!
                    });
                    refresh();
                }} my={theme.space![3]} sx={{width: 'auto', flex: '0'}} variant='muted'>
                    🗓️ Create calendar
                </Button>
            </Flex>
        </>);
    })

    return <div>
        <Flex sx={{flexDirection: "column", gap: 4}}>
            {g}
        </Flex>
        {calendars && calendars.length === 0 &&
            <Box>The bot is not invited to any of your servers (if you just invited it, please re-login).</Box>}

        <Divider/>
        <Link href={apiUrl("/api/auth/join")} target={"_blank"}>Add to server</Link>
    </div>
}

export default CalendarPage;