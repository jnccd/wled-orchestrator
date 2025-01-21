import EditButton from "../EditButton";
import { FormLabel, Input } from "@chakra-ui/react";
import React from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  LedSegmentGroup,
  renameGroup,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";

interface Props {
  group: LedSegmentGroup;
}

const EditGroupButton = ({ group }: Props) => {
  const inputId = "name-input";

  // React Query setup
  const queryClient = useQueryClient();
  const renameGroupMutation = useMutation({
    mutationFn: renameGroup,
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
              id={inputId}
              ref={firstFieldRef}
              defaultValue={group.name ?? ""}
              //id={popoverInputId}
              onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                if (e.key == "Enter") {
                  const newName = (e.target as HTMLInputElement).value;
                  if (!group || !group.id) {
                    console.log("group null??");
                    return;
                  }
                  renameGroupMutation.mutate({
                    groupId: group.id,
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

export default EditGroupButton;
