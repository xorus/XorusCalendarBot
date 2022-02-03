import React, {CSSProperties, useEffect, useRef, useState} from "react";
import {useGuilds} from "../../../lib/useApi";
import {useField} from "formik";
import {ThemeUIStyleObject} from "@theme-ui/css";
import {Box, Flex, Label, useThemeUI} from "theme-ui";
import {GuildAvatar} from "../GuildBanner";

export const GuildSelect = (props: {
    guildField: string,
    channelField: string,
    style: CSSProperties
}) => {
    const [guilds] = useGuilds();
    const [guildField, guildMeta, guildHelpers] = useField(props.guildField);
    const [channelField, channelMeta, channelHelpers] = useField(props.channelField);
    const theme = useThemeUI();

    let selectedGuild = guilds.find(g => {
        if (g.Id === guildField.value) {
            return g;
        }
    });
    let selectedChannel = selectedGuild && selectedGuild.Channels.find(c => {
        if (c.Id === channelField.value) return c;
    }) || null;
    const [opened, setOpened] = useState(false);
    const ref = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const onClick = (e: MouseEvent) => opened && ref.current && !ref.current.contains(e.target as HTMLElement) && setOpened(false);
        document.addEventListener("mousedown", onClick);
        return () => document.removeEventListener("mousedown", onClick);
    }, [opened, ref])

    const fieldSx: ThemeUIStyleObject = {
        ...(theme.theme.forms?.select ?? {}),
        width: "100%",
        padding: 2,
        borderRadius: 5,
        background: 'muted',
        cursor: 'pointer'
    };
    if (opened) {
        fieldSx.borderBottomLeftRadius = 0;
        fieldSx.borderBottomRightRadius = 0;
    }
    const dropdownStyle: ThemeUIStyleObject = {
        flexDirection: "column",
        gap: 3,
        position: "absolute",
        marginTop: '-1px',
        backgroundColor: "background",
        padding: 2,
        minWidth: "100%",
        borderRadius: 5,
        maxHeight: '200px',
        overflowY: 'scroll',
        borderTopLeftRadius: 0,
        borderTopRightRadius: 0,
        borderTop: 'none',
        boxShadow: '1px 1px 2px rgba(0,0,0,0.5)'
    };

    return <div style={{position: "relative", ...(props.style ?? {})}} ref={ref}>
        <Label onClick={() => setOpened(!opened)}>Send notifications to</Label>
        <Box sx={fieldSx} onClick={() => setOpened(!opened)}>
            <Flex sx={{gap: 2, userSelect: 'none'}}>
                {selectedGuild && <GuildAvatar guild={selectedGuild} size={24}/>}
                <span style={{flex: 1}}>
                    {selectedChannel && selectedChannel.Name || 'none selected'}
                </span>
                {opened ? '👈' : '👇'}
            </Flex>
        </Box>

        {opened &&
            <Flex sx={dropdownStyle}>
                {guilds.map(guild => (
                    <div key={guild.Id}>
                        <div style={{display: "flex", gap: "4px"}}>
                            <GuildAvatar guild={guild} size={24}/>
                            <span style={{fontWeight: "bold"}}>{guild.Name}</span>
                        </div>
                        <Box style={{
                            borderLeft: "1px solid var(--theme-ui-colors-gray)",
                            margin: 0,
                            marginLeft: '10px',
                            paddingLeft: '18px',
                            listStyle: 'none'
                        }}>
                            {guild.Channels.filter(channel => !channel.Category).map(channel => (
                                <Box key={channel.Id} sx={{
                                    'cursor': 'pointer',
                                    '&:hover': {
                                        backgroundColor: 'text',
                                        color: 'background'
                                    }
                                }} onClick={_ => {
                                    guildHelpers.setValue(guild.Id);
                                    channelHelpers.setValue(channel.Id);
                                    setOpened(false);
                                }}>
                                    {channel.Name}
                                </Box>
                            ))}
                        </Box>
                    </div>
                ))}
            </Flex>
        }
    </div>
}