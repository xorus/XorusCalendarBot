import React, {ComponentProps, ElementType, ReactNode, useState} from "react";
import {Alert, Box, Button, Card, CardProps, Flex, Grid, Heading, Input, Label, Paragraph, Spinner} from "theme-ui";
import {apiUrl} from "../../lib/apiUrl";
import {userHeaders} from "../../lib/auth";
import {Field, FieldArray, Form, Formik} from "formik";
import {useTheme} from "@theme-ui/style-guide";
import {UserToken} from "../../lib/appState";
import Swal from "sweetalert2";
import {CalendarEntity} from "../../lib/apiObjects";
import {CalendarEditForm} from "./CalendarEditForm";
import Noty from "noty";
import {motion, AnimatePresence} from "framer-motion";
import {ErrorBoundary} from "react-error-boundary";
import {ErrorFallback} from "./ErrorFallback";

export const RefreshButton = (props: {
    refresh: () => void,
    date: string
}) => {
    const [hover, setHover] = useState(false);
    return <Button type="button" variant="muted" onClick={e => {
        props.refresh();
    }} onMouseOver={() => setHover(true)} onMouseOut={() => setHover(false)} title={"Refresh"}>
        {hover ? props.date : '🔃'}
    </Button>;
}


const MotionGrid = motion(Grid);
// const CardProxy = (props: CardProps, ref) => {
//     <Card ...props>{props.children}</Card>
// }
const MotionCard = motion(Card);
export const Calendar = (props: {
    calendar: CalendarEntity,
    user: UserToken,
    reload: () => void,
    refreshCalendar: () => void,
    defaultOpen: boolean,
    canEdit: boolean
}) => {
    let cal = props.calendar;

    const [opened, setOpened] = useState(true||props.defaultOpen);
    const [loading, setLoading] = useState(false);
    const [showAllEvents, setShowAllEvents] = useState(false);
    const theme = useTheme();

    const dtfTime = Intl.DateTimeFormat(navigator.language, {timeStyle: 'long'});
    const dtfFull = Intl.DateTimeFormat(navigator.language, {dateStyle: 'short', timeStyle: 'short'});

    const eventCount = cal.NextOccurrences.length;
    const displayCount = 6;

    // {#  sx={{backgroundColor: theme.colors!.muted}} padding={20}  #}
    return <>
        <Card>
            <Box sx={{float: "right"}}>
                {loading ? <Spinner/> :
                    props.canEdit && <>
                        <RefreshButton refresh={() => props.refreshCalendar()}
                                       date={dtfTime.format(new Date(cal.LastRefresh))}/>
                        <Button type="button" variant="muted" onClick={_ => {
                            Swal.fire({
                                title: 'yeet?',
                                icon: 'warning',
                                showCancelButton: true,
                                confirmButtonColor: '#d33',
                                confirmButtonText: 'YEET.',
                                reverseButtons: true
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    setLoading(true);
                                    fetch(apiUrl("/api/calendar/{id}", {}, {id: cal.Id}), {
                                        ...userHeaders(props.user),
                                        method: "DELETE"
                                    }).then(r => {
                                        setLoading(false);
                                        if (r.ok && r.status === 200) props.reload();
                                        new Noty({
                                            text: "Server returned error: " + r.status + " " + r.statusText,
                                            type: 'error'
                                        }).show();
                                    });
                                }
                            })
                        }} mx={2} title="Yeet">🗑️</Button>
                        <Button onClick={e => {
                            e.preventDefault();
                            setOpened(!opened);
                        }} variant="muted" title="Edit">✏️</Button>
                    </>
                }
            </Box>

            <Heading as={"h3"}>
                {cal.Name ?? "Untitled calendar"}
            </Heading>

            <p>
                🗓️ Next events ({eventCount})
            </p>
            {/*// initial={{opacity: 0}} animate={{ opacity: 1}} exit={{opacity: 0}}*/}

            <motion.div style={{overflow: 'hidden'}}>
                <AnimatePresence exitBeforeEnter>
                    {!opened &&
                        <motion.div initial={{opacity: 0}} animate={{opacity: 1}} exit={{opacity: 0}}
                                    key="eventList" transition={{duration: 0.25}}>
                            <Box sx={{width: '100%'}} my={2}>
                                <Grid columns={[3, '1fr 1fr 1fr']}>
                                    {cal.NextOccurrences.length > 0 ? cal.NextOccurrences.map((o, i) => {
                                        if (!showAllEvents && i >= displayCount) return undefined;
                                        const start = new Date(o.StartTime);
                                        const notify = new Date(o.NotifyTime);
                                        return <Card sx={{
                                            display: "flex",
                                            flexDirection: "column"
                                        }} key={i + o.StartTime}>
                                            <Heading as={"h4"} sx={{
                                                fontSize: 1,
                                                wordBreak: "break-all"
                                            }}>{o.Summary ?? "Event"}</Heading>
                                            <Box sx={{flex: "1"}} my={2}>
                                                {o.Message ? <small>{o.Message}</small> : ""}
                                            </Box>
                                            <Paragraph as={"small"}>
                                                🛎️ {dtfTime.format(notify)}<br/>
                                                ⏲️ {dtfFull.format(start)}
                                            </Paragraph>
                                        </Card>
                                    }) : "No events found."}
                                </Grid>
                                {eventCount > displayCount &&
                                    <Button type={"button"} variant={"muted"} sx={{
                                        width: "100%",
                                        marginTop: 2
                                    }} onClick={e => {
                                        e.preventDefault();
                                        setShowAllEvents(!showAllEvents);
                                    }}>
                                        {showAllEvents ? "Hide" : "Show all events"}
                                    </Button>
                                }
                            </Box>
                        </motion.div>
                    }
                    {opened &&
                        <motion.div initial={{opacity: 0}} animate={{opacity: 1}} exit={{opacity: 0}}
                                    key={"form"} transition={{duration: 0.25}}>
                            {props.canEdit && opened ?
                                <ErrorBoundary FallbackComponent={ErrorFallback}>
                                    <CalendarEditForm calendar={props.calendar} loading={loading => setLoading(loading)}
                                                  saved={props.reload}/>
                                </ErrorBoundary> : null}
                        </motion.div>}
                </AnimatePresence>
            </motion.div>

        </Card>
    </>;
}

const CalendarEvent = (props: {}) => {

}