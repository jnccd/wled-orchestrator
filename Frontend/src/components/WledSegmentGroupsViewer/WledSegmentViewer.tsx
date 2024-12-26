import { HStack, Text } from "@chakra-ui/react";
import { AxiosError, CanceledError } from "axios";
import { WledOrchState } from "../../hooks/useWledOrchState";
import apiClient from "../../services/api-client";
import { components } from "../../types/api";
import Draggable from "../Draggable";
import EditNameButton from "../EditNameButton";

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
          segmentId: elem.classList[0],
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

  const lastAddressByte = s.wledServerAddress?.split(".").slice(-1)[0];

  return (
    <Draggable
      key={s.readonlyId}
      className={s.readonlyId ?? "error-id-less-segment"}
      id={serverButtonIdPrefix + "-" + lastAddressByte}
      onDragEnd={onDragEnd}
    >
      <HStack>
        <Text>{lastAddressByte ?? "Not Found"}</Text>
        <EditNameButton defaultValue={lastAddressByte}></EditNameButton>
      </HStack>
    </Draggable>
  );
};

export default WledSegmentViewer;
