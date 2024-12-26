import useWledOrchState, { WledOrchState } from "../../hooks/useWledOrchState";
import {
  Box,
  FocusLock,
  HStack,
  IconButton,
  Input,
  Popover,
  PopoverArrow,
  PopoverCloseButton,
  PopoverContent,
  PopoverTrigger,
  Text,
  useDisclosure,
} from "@chakra-ui/react";
import Draggable from "../Draggable";
import apiClient from "../../services/api-client";
import { AxiosError, CanceledError } from "axios";
import { EditIcon } from "@chakra-ui/icons";
import React from "react";
import { components } from "../../types/api";

const serverButtonIdPrefix = "server-button";

interface Props {
  ledSegmentClassName: string;
  refreshWledGroupViewer: () => void;
  wledOrchState: WledOrchState;
  segment: components["schemas"]["LedSegment"];
}

const WledSegmentViewer = ({
  segment: s,
  ledSegmentClassName,
  refreshWledGroupViewer: refreshState,
  wledOrchState,
}: Props) => {
  const onDragEnd = (elem: HTMLElement, mousePos: number[]) => {
    const hitGroup = document
      .elementsFromPoint(mousePos[0], mousePos[1])
      .filter((x) => x.classList.contains(ledSegmentClassName));
    const hitGroupData = wledOrchState.ledSegmentGroups?.groups?.filter(
      (x) => x.id === hitGroup[0]?.classList[1]
    );

    apiClient
      .put("/state/moveSegment", null, {
        params: {
          segmentId: elem.classList[1],
          targetGroupId: hitGroupData ? hitGroupData[0]?.id : "",
        },
      })
      .then((_) => {
        refreshState();
      })
      .catch((err: AxiosError) => {
        if (err instanceof CanceledError) return;
        console.log(err);
      });
  };

  const { onOpen, onClose, isOpen } = useDisclosure();
  const firstFieldRef = React.useRef(null);

  return (
    <Draggable
      key={s.readonlyId}
      className={s.readonlyId ?? "error-id-less-segment"}
      id={
        serverButtonIdPrefix +
        "-" +
        s.wledServerAddress?.split(".").slice(-1)[0]
      }
      onDragEnd={onDragEnd}
    >
      <HStack>
        <Text marginRight={3}>
          {s.wledServerAddress?.split(".").slice(-1)[0] ?? "Not Found"}
        </Text>
        <Popover
          isOpen={isOpen}
          onOpen={onOpen}
          onClose={onClose}
          placement="right"
          closeOnBlur={false}
        >
          <PopoverTrigger>
            <IconButton
              onMouseDown={(e: React.MouseEvent) => {
                console.log("hey");
              }}
              size="sm"
              icon={<EditIcon />}
              aria-label={""}
            />
          </PopoverTrigger>
          <PopoverContent p={5}>
            <FocusLock persistentFocus={true}>
              <PopoverArrow />
              <PopoverCloseButton />
              <Input ref={firstFieldRef} id={s.readonlyId ?? ""} />
            </FocusLock>
          </PopoverContent>
        </Popover>
      </HStack>
    </Draggable>
  );
};

export default WledSegmentViewer;
