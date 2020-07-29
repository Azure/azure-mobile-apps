import React from 'react';
import { View, ListView, FlatList, StyleSheet, Text } from 'react-native';
import CustomRow from './CustomRow';

const styles = StyleSheet.create({
    container: {
        flex: 1,
    },
});


const CustomList = ({ itemList }) => (
        <FlatList
                data={itemList}
                renderItem={({ item }) => <CustomRow
                    title={item.title}
                    time={item.time}
                    description={item.description}
                    image_url={item.image_url}
                />}
            />
);

export default CustomList;