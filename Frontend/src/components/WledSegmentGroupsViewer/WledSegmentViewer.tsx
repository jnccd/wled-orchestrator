import { HStack, Text } from "@chakra-ui/react";
import {
  moveSegment,
  wledOrchStateQueryKey,
  getWledOrchState,
  LedSegment,
  renameSegment,
} from "../../hooks/useWledOrchState";
import Draggable from "../Draggable";
import EditNameButton from "../EditNameButton";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

const serverButtonIdPrefix = "server-button";

interface Props {
  ledSegmentClassName: string;
  segment: LedSegment;
}

const WledSegmentViewer = ({ segment, ledSegmentClassName }: Props) => {
  // React Query setup
  const queryClient = useQueryClient();
  const query = useQuery({
    queryKey: [wledOrchStateQueryKey],
    queryFn: getWledOrchState,
  });
  const moveSegmentMutation = useMutation({
    mutationFn: moveSegment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });
  const renameSegmentMutation = useMutation({
    mutationFn: renameSegment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const onDragEnd = (elem: HTMLElement, mousePos: number[]) => {
    const hitElements = document.elementsFromPoint(mousePos[0], mousePos[1]);
    const hitGroup = hitElements.filter((x) =>
      x.classList.contains(ledSegmentClassName)
    );
    const hitGroupData = query.data?.groups?.filter(
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
      key={segment.readonlyId}
      className={segment.readonlyId ?? "error-id-less-segment"}
      id={serverButtonIdPrefix + "-" + lastAddressByte}
      onDragEnd={onDragEnd}
    >
      <HStack>
        <Text>{displayText}</Text>
        <EditNameButton
          defaultValue={displayText}
          onSubmit={(newName) => {
            if (!segment || !segment.readonlyId) {
              console.log("segment null??");
              return;
            }
            renameSegmentMutation.mutate({
              segmentId: segment.readonlyId,
              newName: newName,
            });
          }}
        ></EditNameButton>
      </HStack>
    </Draggable>
  );
};

export default WledSegmentViewer;
