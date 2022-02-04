import {Alert, Box, Button, Card, Flex, Input, Label, Select, Textarea, Themed} from "theme-ui";
import {Field, FieldProps, Form, Formik, FormikProps} from "formik";
import {apiUrl} from "../../lib/apiUrl";
import {userHeaders} from "../../lib/auth";
import React, {ComponentProps, CSSProperties, ElementType, useCallback, useEffect, useRef, useState} from "react";
import {CalendarEntity} from "../../lib/apiObjects";
import {authState, UserToken} from "../../lib/appState";
import {useRecoilState} from "recoil";
import {ChannelSelect} from "./form/ChannelSelect";
import {TimeOffset} from "./form/TimeOffset";
import styled from "@emotion/styled";

const flex = (val ?: string) => ({sx: {flex: val ?? "1 1 auto"}});

const MyTextarea = styled.textarea`
  border: none;
  border-bottom: 2px solid black;
  background-color: var(--theme-ui-colors-background);
  resize: vertical;

  &:focus {
    outline: none;
    border-bottom-color: var(--theme-ui-colors-secondary);
  }
`;

const toFormData = (calendar: CalendarEntity) => {
    calendar = {...calendar};
    calendar.JoinedSentences = calendar.Sentences.join("\n");
    return calendar;
}

const fromFormData = (calendar: CalendarEntity) => {
    calendar.Sentences = calendar.JoinedSentences?.split("\n")?.filter(x => x.trim().length > 0) ?? [];
    delete calendar.JoinedSentences;
    return calendar;
}
export const CalendarEditForm = (props: {
    calendar: CalendarEntity,
    loading: (state: boolean) => void
    saved: () => void
}) => {
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);
    const cal = props.calendar;
    const [user] = useRecoilState<UserToken | null>(authState);
    if (user === null) return <></>;

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

    return <Card>
        {error.length > 0 && <Alert role={"alert"}>{error}</Alert>}
        <Formik initialValues={toFormData(cal)} onSubmit={data => {
            data = fromFormData(data);
            console.log(data);
            setError("");
            setLoading(true);
            props.loading(true);
            fetch(apiUrl("/api/calendar/{id}", {}, {id: cal.Id}), {
                ...userHeaders(user),
                method: "PUT",
                body: JSON.stringify(data)
            }).then(r => {
                setLoading(false);
                props.loading(false);
                if (r.ok && r.status === 200) props.saved();
                else setError("Server returned error: " + r.status + " " + r.statusText)
            });
        }}>
            {({values}) => (
                <Form>
                    <Flex variant="layout.formRow">
                        <I text="Name" boxP={flex("5")}/>
                        <I text="Only process events starting with" entity="CalendarEventPrefix" boxP={flex("5")}/>
                        <I text="Max days" entity="MaxDays" boxP={flex("1")}
                           fieldP={{type: "number"}}/>
                    </Flex>
                    <Box variant="layout.formRow">
                        <I text="Calendar url (accepts caldav/vCal/iCal formats)" entity="CalendarUrl"/>
                    </Box>
                    <Flex variant="layout.formRow">
                        <ChannelSelect style={{flex: 1, minWidth: "40%"}} guildField={"GuildId"}
                                       channelField={"ReminderChannel"}/>
                        <TimeOffset field="ReminderOffsetSeconds" calendarId={cal.Id}/>
                    </Flex>
                    <Box {...flex()}>
                        <Label>Sentences (one per line)</Label>
                        <Field name="JoinedSentences">
                            {({field, form, meta}: FieldProps) =>
                                <Textarea value={field.value} onChange={field.onChange} name="JoinedSentences"
                                          rows={values.Sentences.length + 1}/>
                            }
                        </Field>
                    </Box>
                    <details>
                        <summary>/next command settings</summary>
                        <Box variant="layout.formRow">
                            <I text="NothingPlannedMessage"/>
                        </Box>
                        <Box variant="layout.formRow">
                            <I text="NextDateMessage"/>
                        </Box>
                    </details>
                    <details>
                        <summary>Other settings</summary>
                        <I text="NextSentence" boxP={{my: 2}}/>
                    </details>

                    <Box sx={{textAlign: "right", marginTop: 3}}>
                        <Button type={"submit"} sx={{width: "100%"}}>{loading ? '⌛ Saving...' : '💾 Save'}</Button>
                    </Box>
                </Form>
            )}
        </Formik>
    </Card>;
}