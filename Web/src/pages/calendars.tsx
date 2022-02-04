import React, {useCallback, useEffect, useState} from "react";
import {Avatar, Box, Button, Divider, Flex, Heading, Link, Paragraph, Spinner} from "theme-ui";
import {apiUrl} from "../../lib/apiUrl";
import {useAuth} from "../../lib/auth";
import {useTheme} from "@theme-ui/style-guide";
import {jsonReq, useGuilds} from "lib/useApi";
import {Calendar} from "../components/Calendar";
import {GuildBanner} from "../components/GuildBanner";
import {CalendarEntity} from "../../lib/apiObjects";

const CalendarPage = () => {
    const [user] = useAuth();
    const [calendars, setCalendars] = useState<CalendarEntity[] | null>();
    const [guilds] = useGuilds();
    const theme = useTheme();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const reload = useCallback(async () => {
        console.log("req calendars " + JSON.stringify(user))
        try {
            const json = await jsonReq({url: apiUrl("/api/calendar"), user: user!});
            json && setCalendars(json);
            setLoading(false);
        } catch (e) {
            setError("" + e);
        }
    }, [user]);

    const refreshCalendar = useCallback(async (id: string) => {
        const json = await jsonReq({url: apiUrl("/api/calendar/" + id + "/refresh"), user: user!});
        json && setCalendars(json);
    }, [user]);

    useEffect(() => {
        user && reload()
    }, [user, reload]);

    if (user == null) return null;

    let calendarsPerGuild: Map<string, CalendarEntity[]> = new Map<string, CalendarEntity[]>();
    guilds.forEach(guild => {
        calendarsPerGuild.set(guild.Id, []);
    })

    let g: React.ReactNode[] = [];
    calendars?.forEach(c => {
        let col = calendarsPerGuild.get(c.GuildId) ?? [];
        col.push(c);
        calendarsPerGuild.set(c.GuildId, col);
    });

    calendarsPerGuild.forEach((collection, guildId) => {
        g.push(<>
            <GuildBanner guild={guilds.find((g) => g.Id === guildId)}/>
            <Flex variant={"layout.guildCalendars"}>
                {collection ?
                    collection.map(c => <Calendar key={c.Id} calendar={c} user={user} reload={() => reload()}
                                                  defaultOpen={false && collection.length === 1}
                                                  refreshCalendar={() => refreshCalendar(c.Id)}
                                                  canEdit={true}
                    />) : <Box></Box>}
                <Button type={"button"} onClick={async () => {
                    await jsonReq({
                        url: apiUrl("/api/calendar", {guild: guildId}, {}),
                        method: "POST",
                        body: "",
                        user: user!
                    });
                    reload();
                }} my={theme.space![3]} sx={{width: 'auto', flex: '0'}} variant='muted'>
                    🗓️ Create calendar
                </Button>
            </Flex>
        </>);
    })

    if (loading) {
        return <div style={{textAlign: "center"}}>
            <Spinner/>
        </div>
    }

    if (error) {
        return <div>
            {error}<br/>
            <Button onClick={() => reload()} type="button">Retry</Button><br/>
            <Spinner/>
        </div>
    }

    return <div>

        <Flex sx={{flexDirection: "column", gap: 4}}>
            {g}
        </Flex>
        {guilds && guilds.length === 0 &&
            <Box>The bot is not invited to any of your servers (if you just invited it, please re-login).</Box>}

        <Link href={apiUrl("/api/auth/invite")} target={"_blank"}>Invite to server (requires re-login after
            inviting)</Link>

        <Divider/>
        <Heading as="h2" my={3}>How to use</Heading>
        <Paragraph my={2}>
            To force an event to show up, prefix it by <kbd>!event</kbd> or assign it to the &quot;Discord
            Event&quot; category (categories are not available on GMail).
        </Paragraph>
        <Paragraph my={2}>
            To force an event to use a certain message, prefix the content by <kbd>!event-message</kbd>
            &nbsp;(message can only be on one line)
        </Paragraph>
    </div>
}

export default CalendarPage;