import React, {ComponentProps, ElementType, useState} from "react";
import {Alert, Box, Button, Card, Flex, Grid, Heading, Input, Label, Paragraph, Spinner} from "theme-ui";
import {apiUrl} from "../../lib/apiUrl";
import {userHeaders} from "../../lib/auth";
import {Field, FieldArray, Form, Formik} from "formik";
import {useTheme} from "@theme-ui/style-guide";
import {UserToken} from "../../lib/appState";
import Swal from "sweetalert2";
import {CalendarEntity} from "../../lib/apiObjects";

export const RefreshButton = (props: {
    refresh: () => void,
    date: string
}) => {
    const [hover, setHover] = useState(false);
    return <Button type="button" variant="muted" onClick={e => {
        props.refresh();
    }} sx={{float: "right"}} onMouseOver={() => setHover(true)} onMouseOut={() => setHover(false)}>
        {hover ? props.date : 'Refresh'}
    </Button>;
}

export const Calendar = (props: {
    calendar: CalendarEntity,
    user: UserToken,
    reload: () => void,
    refreshCalendar: () => void,
    defaultOpen: boolean
}) => {
    let cal = props.calendar;

    const [opened, setOpened] = useState(props.defaultOpen);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState("");
    const [showAllEvents, setShowAllEvents] = useState(false);
    const theme = useTheme();

    const I = (x: {
        text: string,
        entity?: string,
        labelP?: ComponentProps<typeof Field>,
        fieldP?: ComponentProps<typeof Field>,
        block?: ElementType,
        boxP?: ComponentProps<typeof Box>,
    }) => {
        let entity = x.entity;
        if (!entity) entity = x.text;
        return <Box as={x.block ?? "div"} {...(x.boxP ?? {})}>
            <Label htmlFor={"cal-" + cal.Id + "-" + entity} {...(x.labelP ?? {})}>{x.text}</Label>
            <Field as={Input} id={"cal-" + cal.Id + "-" + entity} name={entity} {...(x.fieldP ?? {})}/>
        </Box>
    }

    const dtfTime = Intl.DateTimeFormat(navigator.language, {timeStyle: 'long'});
    const dtfFull = Intl.DateTimeFormat(navigator.language, {dateStyle: 'short', timeStyle: 'short'});

    const eventCount = cal.NextOccurrences.length;

    // {#  sx={{backgroundColor: theme.colors!.muted}} padding={20}  #}
    const flex = (val ?: string) => ({sx: {flex: val ?? "1 1 auto"}});
    return <>
        <Card>
            {saving && <Spinner sx={{float: "right"}}/>}

            <Button type="button" variant="muted" onClick={e => {
                Swal.fire({
                    title: 'yeet?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    confirmButtonText: 'YEET.',
                    reverseButtons: true
                }).then((result) => {
                    if (result.isConfirmed) {
                        setError("");
                        setSaving(true);
                        fetch(apiUrl("/api/calendar/{id}", {}, {id: cal.Id}), {
                            ...userHeaders(props.user),
                            method: "DELETE"
                        }).then(r => {
                            setSaving(false);
                            if (r.ok && r.status === 200) props.reload();
                            else setError("Server returned error: " + r.status + " " + r.statusText)
                        });
                    }
                })
            }} sx={{float: "right"}} mx={2}>🗑️</Button>
            <RefreshButton refresh={() => props.refreshCalendar()} date={dtfTime.format(new Date(cal.LastRefresh))}/>

            <Heading as={"h3"}>
                {cal.Name ?? "Untitled calendar"}
            </Heading>

            <p>
                🗓️ Next events ({eventCount})
            </p>

            <Box sx={{width: '100%'}} my={2}>
                <Grid columns={[3, '1fr 1fr 1fr']}>
                    {cal.NextOccurrences.length > 0 ? cal.NextOccurrences.map((o, i) => {
                        if (!showAllEvents && i > 2) return undefined;
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
                                {o.ForcedMessage ? <small>{o.ForcedMessage}</small> : ""}
                            </Box>
                            <Paragraph as={"small"}>
                                🛎️ {dtfTime.format(notify)}<br/>
                                ⏲️ {dtfFull.format(start)}
                            </Paragraph>
                        </Card>
                    }) : "No events found."}
                </Grid>
                {eventCount > 3 &&
                    <Button type={"button"} variant={"muted"} sx={{
                        width: "100%",
                        marginTop: 2
                    }} onClick={e => {
                        e.preventDefault();
                        setShowAllEvents(!showAllEvents);
                    }}>
                        {showAllEvents ? "Hide" : "Show more events"}
                    </Button>
                }
            </Box>

            <Button onClick={e => {
                e.preventDefault();
                setOpened(!opened);
            }} sx={{
                width: "100%",
                marginTop: '2',
                marginBottom: opened ? '2' : '0',
            }} variant={"cardOpen"}>{saving ? 'Saving...' : (opened ? '🔽 Close' : '▶️ Edit')}</Button>

            {opened ? <Card>
                {error.length > 0 && <Alert role={"alert"}>{error}</Alert>}
                <Formik initialValues={cal} onSubmit={data => {
                    setError("");
                    setSaving(true);
                    fetch(apiUrl("/api/calendar/{id}", {}, {id: cal.Id}), {
                        ...userHeaders(props.user),
                        method: "PUT",
                        body: JSON.stringify(data)
                    }).then(r => {
                        setSaving(false);
                        if (r.ok && r.status === 200) props.reload();
                        else setError("Server returned error: " + r.status + " " + r.statusText)
                    });
                }}>
                    {({values}) => (
                        <Form>
                            <Flex variant="layout.formRow">
                                {I({text: "Name", boxP: flex('5')})}
                                {I({text: "CalendarEventPrefix", boxP: flex('5')})}
                                {I({text: "MaxDays", boxP: flex('1'), fieldP: {type: "number"}})}
                            </Flex>
                            <Box variant="layout.formRow">
                                {I({text: "CalendarUrl"})}
                            </Box>
                            <Box variant="layout.formRow">
                                {I({text: "NextDateMessage"})}
                            </Box>
                            <Box variant="layout.formRow">
                                {I({text: "NothingPlannedMessage"})}
                            </Box>
                            <Flex variant="layout.formRow">
                                {I({text: "GuildId", boxP: flex()})}
                                {I({text: "ReminderChannel", boxP: flex()})}
                            </Flex>
                            <Flex variant="layout.formRow">
                                {I({text: "ReminderOffsetSeconds", boxP: flex(), fieldP: {type: "number", step: 60}})}
                                <Box>
                                    <Label htmlFor={`cal${cal.Id}+ReminderOffsetSeconds_h`}>Hours :</Label>
                                    <Input id={`cal${cal.Id}+ReminderOffsetSeconds_h`}
                                           value={Math.floor(Math.abs(values.ReminderOffsetSeconds) / 60 / 60)}
                                           readOnly={true}/>
                                </Box>
                                <Box>
                                    <Label htmlFor={`cal${cal.Id}+ReminderOffsetSeconds_m`}>Minutes</Label>
                                    <Input id={`cal${cal.Id}+ReminderOffsetSeconds_m`}
                                           value={Math.floor(Math.abs(values.ReminderOffsetSeconds) / 60 % 60)}
                                           readOnly={true}/>
                                </Box>
                            </Flex>
                            <Flex variant="layout.formRow">
                                <FieldArray name="Sentences">
                                    {({insert, remove, push}) => (
                                        <Box {...flex()}>
                                            <Label>Sentences ({values.Sentences.length})</Label>
                                            <Card>
                                                <Flex sx={{flexDirection: 'column', gap: '2'}}>
                                                    {values.Sentences.length > 0 && values.Sentences.map((sentence, index) => {
                                                        return <>
                                                            <Flex variant={"layout.formRow"} sx={{gap: '1'}}>
                                                                {/*<Button onClick={e => {*/}
                                                                {/*}} disabled={true}>&uarr;</Button>*/}
                                                                {/*<Button onClick={e => {*/}
                                                                {/*}}>&darr;</Button>*/}
                                                                <I text={`Sentences.${index}`}
                                                                   boxP={flex("1")}
                                                                   labelP={{sx: {display: "none"}}}
                                                                   fieldP={{placeholder: ""}}
                                                                />
                                                                <Button type="button" onClick={e => {
                                                                    remove(index)
                                                                }} variant="secondary">❌️</Button>
                                                            </Flex>
                                                        </>
                                                    })}
                                                </Flex>
                                                <Button type="button" onClick={() => push("")} variant="secondary" sx={{
                                                    width: '100%',
                                                    fontSize: '1',
                                                    padding: '0',
                                                    marginTop: '1'
                                                }}>
                                                    Add Sentence
                                                </Button>
                                            </Card>
                                        </Box>
                                    )}
                                </FieldArray>
                            </Flex>
                            {I({text: "NextSentence", boxP: {my: 2}})}

                            <Box sx={{textAlign: "right", marginTop: 3}}>
                                <Button type={"submit"} sx={{width: "100%"}}>💾 Save</Button>
                            </Box>
                        </Form>
                    )}
                </Formik>
            </Card> : null}
        </Card>
    </>;
}

const CalendarEvent = (props: {}) => {

}