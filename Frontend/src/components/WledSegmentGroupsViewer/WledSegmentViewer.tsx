import { HStack, Text } from "@chakra-ui/react";
import {
  moveSegment,
  wledOrchStateQueryKey,
  getWledOrchState,
  LedSegment,
} from "../../hooks/useWledOrchApi";
import Draggable from "../Draggable";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import EditSegmentButton from "./EditSegmentButton";

const serverButtonIdPrefix = "server-button";

interface Props {
  ledSegmentClassName: string;
  segment: LedSegment;
}

const WledSegmentViewer = ({ segment, ledSegmentClassName }: Props) => {
  // React Query setup
  const queryClient = useQueryClient();
  const wledOrchStateQuery = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const moveSegmentMutation = useMutation({
    mutationFn: moveSegment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const onDragEnd = (elem: HTMLElement, mousePos: number[]) => {
    const hitElements = document.elementsFromPoint(mousePos[0], mousePos[1]);
    const hitGroup = hitElements.filter((x) =>
      x.classList.contains(ledSegmentClassName)
    );
    const hitGroupData = wledOrchStateQuery.data?.groups?.filter(
      (x) => x.id === hitGroup[0]?.classList[1]
    );

    moveSegmentMutation.mutate({
      segmentId: elem.classList[0],
      targetGroupId: hitGroupData ? hitGroupData[0]?.id ?? null : null,
    });
  };

  const lastAddressByte = segment.wledServerAddress?.split(".").slice(-1)[0];
  const displayText = segment.name ?? lastAddressByte ?? "Not Found";

  return (
    <Draggable
      key={segment.id}
      className={segment.id ?? "error-id-less-segment"}
      id={serverButtonIdPrefix + "-" + lastAddressByte}
      onDragEnd={onDragEnd}
    >
      <HStack>
        <Text>{displayText}</Text>
        <EditSegmentButton segment={segment}></EditSegmentButton>
      </HStack>
    </Draggable>
  );
};

export default WledSegmentViewer;
