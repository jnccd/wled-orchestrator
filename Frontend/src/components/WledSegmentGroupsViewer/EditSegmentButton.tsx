import EditButton from "../EditButton";
import { FormLabel, Input } from "@chakra-ui/react";
import React from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  LedSegment,
  renameSegment,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";

interface Props {
  segment: LedSegment;
}

const EditSegmentButton = ({ segment }: Props) => {
  const inputId = "name-input";

  // React Query setup
  const queryClient = useQueryClient();
  const renameSegmentMutation = useMutation({
    mutationFn: renameSegment,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  return (
    <EditButton
      children={(_a, _b, onClose, firstFieldRef) => {
        return (
          <>
            <FormLabel htmlFor={inputId}>Name:</FormLabel>
            <Input
              id={"name-input"}
              ref={firstFieldRef}
              defaultValue={segment.name ?? ""}
              //id={popoverInputId}
              onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                if (e.key == "Enter") {
                  const newName = (e.target as HTMLInputElement).value;
                  if (!segment || !segment.readonlyId) {
                    console.log("segment null??");
                    return;
                  }
                  renameSegmentMutation.mutate({
                    segmentId: segment.readonlyId,
                    newName: newName,
                  });

                  onClose();
                }
              }}
            />
          </>
        );
      }}
    ></EditButton>
  );
};

export default EditSegmentButton;
