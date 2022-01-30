import {deep, base, funk} from '@theme-ui/presets';
import {Theme} from 'theme-ui';

const theme: Theme = {
    ...base,
    config: {
        initialColorModeName: 'dark',
    },
    fonts: {
        body: "Cabin, sans-serif",
        heading: "Mochiy Pop P One, sans-serif",
        monospace: "Menlo, monospace"
    },
    colors: {
        ...funk.colors,
        danger: "hsl(10, 80%, 50%)",
        modes: {
            dark: {
                ...deep.colors,
                danger: "hsl(10, 80%, 50%)",
            },
        },
    },
    layout: {
        container: {
            maxWidth: 1024,
            mx: 'auto',
            py: 3,
            px: 4,
        },
        formRow: {
            width: '100%',
            gap: '3',
            marginBottom: '2'
        },
        guildPlaceholderAvatar: {
            display: 'inline-block',
            width: 48,
            height: 46,
            fontSize: 26,
            padding: '0px',
            paddingTop: '2px',
            overflow: 'hidden',
            textAlign: 'center',
            borderRadius: 48,
            backgroundColor: '#36393f',
            color: 'white'
        },
        guildCalendars: {
            paddingLeft: 4,
            borderLeft: '8px solid black',
            borderLeftColor: 'muted',
            marginLeft: '20px',
            marginBottom: 4,
            flexDirection: "column",
            gap: 3
        }
    },
    images: {
        guild: {
            width: 48,
            height: 48,
            borderRadius: 48,
        }
    },
    cards: {
        primary: {
            padding: 20,
            borderRadius: 4,
            boxShadow: '0 0 8px rgba(0, 0, 0, 0.125)',
            backgroundColor: 'muted'
        },
    },
    buttons: {
        primary: {
            fontFamily: 'body',
            cursor: 'pointer',
        },
        muted: {
            fontFamily: 'body',
            cursor: 'pointer',
            backgroundColor: 'muted',
            color: 'text'
        },
        secondary: {
            fontFamily: 'body',
            cursor: 'pointer',
            color: 'background',
            bg: 'secondary',
        },
        danger: {
            fontFamily: 'body',
            cursor: 'pointer',
            bg: 'danger',
        },
        cardOpen: {
            fontFamily: 'body',
            cursor: 'pointer',
            bg: 'muted',
            text: 'text',
            textAlign: 'left',
            color: 'text'
        }
    },
};

// links: {
//     nav: {
//         fontFamily: 'body',
//     },
// },

export default theme;
