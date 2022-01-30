import {Avatar, Box, Flex, Heading} from "theme-ui";
import React from "react";
import {Guild} from "../../lib/apiObjects";

export const GuildBanner = (props: { guild?: Guild }) => {
    if (!props.guild) return <Box>unknown guild</Box>;
    return <Flex sx={{gap: '3', alignItems: 'center'}}>
        {props.guild.IconUrl ?
            <Avatar variant="images.guild" src={props.guild.IconUrl} sx={{borderRadius: 10}}/>
            : <Box variant="layout.guildPlaceholderAvatar" sx={{borderRadius: 10}}>{
                props.guild.Name.split(' ').map((n, k) => k < 2 ? n.substring(0, 1) : '').join('')
            }</Box>}
        <Heading as="h2" sx={{flex: '1 1 auto'}}>{props.guild.Name}</Heading>
    </Flex>;
}
