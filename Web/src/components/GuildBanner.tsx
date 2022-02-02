import {Avatar, Box, Flex, Heading} from "theme-ui";
import React from "react";
import {Guild} from "../../lib/apiObjects";

export const GuildAvatar = (props: { guild: Guild, size: number }) => props.guild.IconUrl ?
    <Avatar variant="images.guild" src={props.guild.IconUrl} sx={{
        width: props.size + 'px',
        height: props.size + 'px',
        borderRadius: 10
    }}/>
    : <Box sx={{
        display: 'inline-block',
        width: props.size + 'px',
        height: (props.size - 2) + 'px',
        fontSize: (props.size / 48) * 26 + 'px',
        padding: '0px',
        paddingTop: '2px',
        overflow: 'hidden',
        textAlign: 'center',
        backgroundColor: '#36393f',
        color: 'white',
        borderRadius: 10
    }}>{
        props.guild.Name.split(' ').map((n, k) => k < 2 ? n.substring(0, 1) : '').join('')
    }</Box>;

export const GuildBanner = (props: { guild?: Guild }) => {
    if (!props.guild) return <Box>unknown guild</Box>;
    return <Flex sx={{gap: '3', alignItems: 'center'}}>
        <GuildAvatar guild={props.guild} size={48}/>
        <Heading as="h2" sx={{flex: '1 1 auto'}}>{props.guild.Name}</Heading>
    </Flex>;
}
