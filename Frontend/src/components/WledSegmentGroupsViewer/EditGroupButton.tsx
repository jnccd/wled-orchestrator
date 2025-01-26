import EditButton from "../EditButton";
import {
  Button,
  Divider,
  FormLabel,
  HStack,
  Input,
  useToast,
  VStack,
} from "@chakra-ui/react";
import React from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  deleteGroup,
  LedSegmentGroup,
  renameGroup,
  wledOrchStateQueryKey,
} from "../../hooks/useWledOrchApi";
import { AxiosError } from "axios";

interface Props {
  group: LedSegmentGroup;
}

const EditGroupButton = ({ group }: Props) => {
  const inputId = "name-input";
  const toast = useToast();

  // React Query setup
  const queryClient = useQueryClient();
  const renameGroupMutation = useMutation({
    mutationFn: renameGroup,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });
  const deleteGroupMutation = useMutation({
    mutationFn: deleteGroup,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [wledOrchStateQueryKey] });
    },
  });

  const inputSubmit = (newName: string, onClose: () => void) => {
    if (!group || !group.id) {
      console.log("group null??");
      return;
    }
    renameGroupMutation.mutate({
      groupId: group.id,
      newName: newName,
    });

    onClose();
  };

  return (
    <EditButton
      children={(_a, _b, onClose, firstFieldRef) => {
        return (
          <VStack alignItems={"left"}>
            <FormLabel textAlign={"left"} htmlFor={inputId}>
              Name:
            </FormLabel>
            <HStack>
              <Input
                id={inputId}
                ref={firstFieldRef}
                defaultValue={group.name ?? ""}
                onKeyDown={(e: React.KeyboardEvent<HTMLInputElement>) => {
                  if (e.key === "Enter") {
                    const newName = (e.target as HTMLInputElement).value;
                    inputSubmit(newName, onClose);
                  }
                }}
              />
              <Button
                onClick={() => {
                  const newName = (
                    document.getElementById(inputId) as HTMLInputElement
                  ).value;
                  inputSubmit(newName, onClose);
                }}
              >
                Submit
              </Button>
            </HStack>
            <Divider marginY={2} width="90%" marginX="auto" opacity={1} />
            <Button
              colorScheme="red"
              onClick={() =>
                deleteGroupMutation.mutate(
                  {
                    groupId: group.id ?? "",
                  },
                  {
                    onError: (error) => {
                      toast.closeAll();
                      toast({
                        title:
                          ((error as AxiosError).response?.data as string) ??
                          "Error!",
                        status: "error",
                        position: "top",
                        variant: "left-accent",
                        isClosable: true,
                      });
                    },
                  }
                )
              }
            >
              Delete
            </Button>
          </VStack>
        );
      }}
    ></EditButton>
  );
};

export default EditGroupButton;
